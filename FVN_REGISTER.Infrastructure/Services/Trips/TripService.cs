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

    public TripService(IUnitOfWork uow, ICurrentUserService currentUser,
        IApprovalWorkflowOrchestrator<TripRequestSubject> workflow,
        IAuthorizationService authorization)
    {
        _uow = uow;
        _currentUser = currentUser;
        _workflow = workflow;
        _authorization = authorization;
    }

    public async Task<TripRequestDto> CreateDraftAsync(CreateTripRequestDto request, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        ValidatePeriod(request.StartDate, request.EndDate);

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

    public async Task<TripRequestDto> SubmitAsync(int requestId, CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");

        var entity = await _uow.Repository<F03TripRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy đăng ký công tác.");

        if (!string.Equals(entity.EmployeeCode, user.EmployeeCode, StringComparison.OrdinalIgnoreCase) && !user.IsAdmin)
            throw new UnauthorizedAccessException("Bạn không có quyền gửi đăng ký này.");

        if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision)
            throw new InvalidOperationException("Chỉ đăng ký Nháp/NeedsRevision mới được gửi duyệt.");

        ValidatePeriod(entity.StartDate, entity.EndDate);
        if (string.IsNullOrWhiteSpace(entity.Destination) || string.IsNullOrWhiteSpace(entity.Purpose))
            throw new InvalidOperationException("Địa điểm và mục đích công tác là bắt buộc.");

        entity.RequestStatus = ApprovalStatus.Pending;
        await _uow.SaveChangesAsync(ct);

        var employee = await _uow.Repository<FVN_REGISTER.Core.Entities.HR.F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == entity.EmployeeCode)
            .Select(x => new { x.DeptCode, x.PositionCode })
            .FirstOrDefaultAsync(ct);

        var context = ApprovalBuildContext.ForTrip(
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

    private static void ValidatePeriod(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
    }
}
