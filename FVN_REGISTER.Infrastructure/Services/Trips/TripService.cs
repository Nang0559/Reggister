using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Trips;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Trips;

public sealed class TripService : ITripService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IApprovalWorkflowOrchestrator<TripRequestSubject> _workflow;
    private readonly IAuthorizationService _authorization;
    private readonly IApprovalSelectionService _approvalSelections;

    public TripService(IUnitOfWork uow, ICurrentUserService currentUser,
        IApprovalWorkflowOrchestrator<TripRequestSubject> workflow,
        IAuthorizationService authorization,
        IApprovalSelectionService approvalSelections)
    {
        _uow = uow;
        _currentUser = currentUser;
        _workflow = workflow;
        _authorization = authorization;
        _approvalSelections = approvalSelections;
    }

    public async Task<TripRequestDto> CreateDraftAsync(CreateTripRequestDto request, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        ValidateRequest(request);

        if (!await _authorization.CanAccessAsync(user, SecurityFunctionCodes.TripCreate, user.EmployeeCode, user.DeptCode, ct))
            throw new UnauthorizedAccessException("Bạn không có quyền tạo đăng ký công tác theo phạm vi dữ liệu được cấp.");

        var employeeCode = user.EmployeeCode
            ?? throw new InvalidOperationException("Tài khoản chưa có EmployeeCode.");

        var entity = new F03TripRequest
        {
            EmployeeCode = employeeCode,
            DeptCode = user.DeptCode,
            CreatedBy = user.UserId,
            TripCode = $"TRIP-{Guid.NewGuid():N}"[..30],
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Destination = request.Destination.Trim(),
            Purpose = request.Purpose.Trim(),
            CustomerOrPartner = request.CustomerOrPartner?.Trim(),
            TransportMethod = request.TransportMethod?.Trim(),
            CompanionEmployeeCodes = request.CompanionEmployeeCodes?.Trim(),
            EstimatedCost = request.EstimatedCost,
            Accommodation = request.Accommodation?.Trim(),
            Note = request.Note?.Trim(),
            RequestStatus = ApprovalStatus.Draft
        };

        await _uow.Repository<F03TripRequest>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return await MapAsync(entity, ct);
    }

    public async Task<TripRequestDto> SubmitAsync(int requestId, List<ApprovalSelectionDto>? approvalSelections = null, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");

        var entity = await _uow.Repository<F03TripRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy đăng ký công tác.");

        if (!await _authorization.CanAccessAsync(user, SecurityFunctionCodes.TripEdit, entity.EmployeeCode, entity.DeptCode, ct))
            throw new UnauthorizedAccessException("Bạn không có quyền gửi đăng ký này theo phạm vi dữ liệu được cấp.");

        if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision)
            throw new InvalidOperationException("Chỉ đăng ký Nháp/NeedsRevision mới được gửi duyệt.");

        ValidatePeriod(entity.StartDate, entity.EndDate);
        if (string.IsNullOrWhiteSpace(entity.Destination) || string.IsNullOrWhiteSpace(entity.Purpose))
            throw new InvalidOperationException("Địa điểm và mục đích công tác là bắt buộc.");

        if (approvalSelections != null && approvalSelections.Count > 0)
            await _approvalSelections.ReplaceAsync(RequestModule.Trip, entity.Id, approvalSelections, user.UserId, ct);

        entity.RequestStatus = ApprovalStatus.Pending;
        await _uow.SaveChangesAsync(ct);

        var employee = await _uow.Repository<FVN_REGISTER.Core.Entities.HR.F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == entity.EmployeeCode)
            .Select(x => new { x.DeptCode, x.PositionCode })
            .FirstOrDefaultAsync(ct);

        var context = ApprovalBuildContext.ForTrip(
            entity.Id,
            entity.EmployeeCode,
            entity.DeptCode ?? employee?.DeptCode ?? string.Empty,
            employee?.PositionCode ?? string.Empty);

        await _workflow.InitApprovalAsync(entity.Id, context, ct);
        return await MapAsync(entity, ct);
    }

    public async Task<TripRequestDto?> GetAsync(int requestId, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");

        var entity = await _uow.Repository<F03TripRequest>().Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);
        if (entity == null) return null;
        if (!await _authorization.CanAccessAsync(user, SecurityFunctionCodes.TripView, entity.EmployeeCode, entity.DeptCode, ct))
            throw new UnauthorizedAccessException("Bạn không có quyền xem đăng ký này.");

        return await MapAsync(entity, ct);
    }

    public async Task<List<TripRequestDto>> GetMineAsync(CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        var scope = await _authorization.GetScopeAsync(user.UserId, SecurityFunctionCodes.TripView, ct);
        var query = _uow.Repository<F03TripRequest>().Query().AsNoTracking()
            .Where(x => x.IsActive == true);

        if (scope == AuthorizationScopeCodes.Own || scope == AuthorizationScopeCodes.Employee)
            query = query.Where(x => x.EmployeeCode == user.EmployeeCode);
        else if (scope == AuthorizationScopeCodes.Department)
            query = query.Where(x => x.DeptCode == user.DeptCode);
        else if (scope != AuthorizationScopeCodes.All)
            query = query.Where(x => false);

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var codes = entities.Select(x => x.EmployeeCode).Distinct().ToList();
        var names = await _uow.Repository<FVN_REGISTER.Core.Entities.HR.F03Employee>().Query().AsNoTracking()
            .Where(x => codes.Contains(x.EmployeeCode))
            .Select(x => new { x.EmployeeCode, x.EmployeeName })
            .ToDictionaryAsync(x => x.EmployeeCode, x => x.EmployeeName, ct);

        return entities.Select(x => ToDto(x, names.TryGetValue(x.EmployeeCode, out var n) ? n : null)).ToList();
    }

    private async Task<TripRequestDto> MapAsync(F03TripRequest x, CancellationToken ct)
    {
        var name = await _uow.Repository<FVN_REGISTER.Core.Entities.HR.F03Employee>().Query().AsNoTracking()
            .Where(e => e.EmployeeCode == x.EmployeeCode)
            .Select(e => e.EmployeeName)
            .FirstOrDefaultAsync(ct);
        return ToDto(x, name);
    }

    private static TripRequestDto ToDto(F03TripRequest x, string? employeeName) => new()
    {
        Id = x.Id,
        TripCode = x.TripCode,
        EmployeeCode = x.EmployeeCode,
        EmployeeName = employeeName,
        DeptCode = x.DeptCode,
        RequestStatus = x.RequestStatus,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        Destination = x.Destination,
        Purpose = x.Purpose,
        CustomerOrPartner = x.CustomerOrPartner,
        TransportMethod = x.TransportMethod,
        CompanionEmployeeCodes = x.CompanionEmployeeCodes,
        EstimatedCost = x.EstimatedCost,
        Accommodation = x.Accommodation,
        Note = x.Note
    };


    public async Task<TripRequestDto> UpdateDraftAsync(int requestId, CreateTripRequestDto request, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        var entity = await _uow.Repository<F03TripRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy đăng ký công tác.");

        if (!await _authorization.CanAccessAsync(user, SecurityFunctionCodes.TripEdit, entity.EmployeeCode, entity.DeptCode, ct))
            throw new UnauthorizedAccessException("Bạn không có quyền sửa đăng ký này.");

        if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision)
            throw new InvalidOperationException("Chỉ Draft/NeedsRevision mới được sửa.");

        ValidateRequest(request);
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Destination = request.Destination.Trim();
        entity.Purpose = request.Purpose.Trim();
        entity.CustomerOrPartner = request.CustomerOrPartner?.Trim();
        entity.TransportMethod = request.TransportMethod?.Trim();
        entity.CompanionEmployeeCodes = request.CompanionEmployeeCodes?.Trim();
        entity.EstimatedCost = request.EstimatedCost;
        entity.Accommodation = request.Accommodation?.Trim();
        entity.Note = request.Note?.Trim();
        entity.ModifiedBy = user.UserId;
        entity.ModifiedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
        return await MapAsync(entity, ct);
    }

    public async Task CancelAsync(int requestId, string reason, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        var entity = await _uow.Repository<F03TripRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy đăng ký công tác.");

        if (!await _authorization.CanAccessAsync(user, SecurityFunctionCodes.TripEdit, entity.EmployeeCode, entity.DeptCode, ct))
            throw new UnauthorizedAccessException("Bạn không có quyền hủy đăng ký này.");
        if (entity.RequestStatus == ApprovalStatus.Approved || entity.RequestStatus == ApprovalStatus.Rejected || entity.RequestStatus == ApprovalStatus.Cancelled)
            throw new InvalidOperationException("Đăng ký đã kết thúc, không thể hủy.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Lý do hủy không được để trống.");

        entity.RequestStatus = ApprovalStatus.Cancelled;
        entity.IsActive = false;
        entity.ModifiedBy = user.UserId;
        entity.ModifiedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
    }

    private static void ValidateRequest(CreateTripRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidatePeriod(request.StartDate, request.EndDate);
        if (string.IsNullOrWhiteSpace(request.Destination))
            throw new ArgumentException("Địa điểm công tác là bắt buộc.");
        if (string.IsNullOrWhiteSpace(request.Purpose))
            throw new ArgumentException("Mục đích công tác là bắt buộc.");
        if (request.EstimatedCost.HasValue && request.EstimatedCost.Value < 0)
            throw new ArgumentException("Chi phí dự kiến không được âm.");
    }

    private static void ValidatePeriod(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
    }
}
