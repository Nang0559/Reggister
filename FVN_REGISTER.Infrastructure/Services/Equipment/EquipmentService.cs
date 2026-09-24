using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Equipment;

public sealed class EquipmentService : IEquipmentService
{
    private readonly IUnitOfWork _uow; private readonly ICurrentUserService _currentUser; private readonly IApprovalWorkflowOrchestrator<EquipmentRequestSubject> _workflow; private readonly IAuthorizationService _authorization; private readonly IApprovalSelectionService _approvalSelections;
    public EquipmentService(IUnitOfWork uow, ICurrentUserService currentUser, IApprovalWorkflowOrchestrator<EquipmentRequestSubject> workflow, IAuthorizationService authorization, IApprovalSelectionService approvalSelections) { _uow = uow; _currentUser = currentUser; _workflow = workflow; _authorization = authorization; _approvalSelections = approvalSelections; }
    public async Task<ServiceResult<bool>> HasModuleAccessAsync(CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            return ServiceResult<bool>.Ok(
                await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct));
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<bool>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<List<EquipmentApproverDto>>> GetApproversAsync(
        string deptCode,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireModuleUser();
            if (string.IsNullOrWhiteSpace(deptCode))
                return ServiceResult<List<EquipmentApproverDto>>.Fail("Bộ phận là bắt buộc.");

            await EnsureScopeAsync(
                user, SecurityFunctionCodes.EquipmentView,
                user.EmployeeCode, deptCode, ct);

            var data = await _uow.Repository<F03Approver>()
                .Query()
                .AsNoTracking()
                .Where(x => x.RequestType == RequestModule.Equipment &&
                            x.IsActive == true &&
                            (x.ApproveForDeptCode == deptCode ||
                             x.ApproveForDeptCode == ApproveForDept.All))
                .OrderBy(x => x.Level)
                .ThenBy(x => x.ApproverName)
                .Select(x => new EquipmentApproverDto
                {
                    ApproverCode = x.ApproverCode,
                    ApproverName = x.ApproverName,
                    ApproverEmail = x.ApproverEmail,
                    Level = x.Level,
                    RoleName = x.RoleName,
                    ApproveForDeptCode = x.ApproveForDeptCode
                })
                .ToListAsync(ct);

            return ServiceResult<List<EquipmentApproverDto>>.Ok(data);
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<EquipmentApproverDto>>.Fail(ex.Message); }
        catch (ArgumentException ex) { return ServiceResult<List<EquipmentApproverDto>>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<EquipmentRequestDto>> CreateRegistrationDraftAsync(
        CreateEquipmentRegistrationDto request,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireModuleUser();
            ValidateRegistration(request);

            await EnsureScopeAsync(
                user, SecurityFunctionCodes.EquipmentCreate,
                user.EmployeeCode, request.DeptCode, ct);

            await EnsureDepartmentScopeForOwnedDataAsync(user, request.DeptCode, ct);

            if (request.ApprovalSelections == null ||
                request.ApprovalSelections.Count == 0)
                return ServiceResult<EquipmentRequestDto>.Fail(
                    "Vui lòng chọn người phê duyệt cho từng cấp.");

            var entity = new F03EquipmentRequest
            {
                RequestKind = EquipmentRequestKind.Registration,
                EmployeeCode = user.EmployeeCode ?? string.Empty,
                DeptCode = request.DeptCode,
                RequestStatus = ApprovalStatus.Draft,
                CreatedBy = user.UserId,
                OperatorUserId = user.UserId,
                SelectedApproverCode = request.ApprovalSelections
                    .OrderBy(x => x.Level)
                    .Select(x => x.ApproverCode)
                    .FirstOrDefault()
                    ?? request.SelectedApproverCode?.Trim()
                    ?? string.Empty,
                QrToken = Convert.ToHexString(Guid.NewGuid().ToByteArray()) +
                          Guid.NewGuid().ToString("N"),
                EquipmentName = request.EquipmentName.Trim(),
                Specification = request.Specification?.Trim(),
                SerialNumber = request.SerialNumber?.Trim(),
                AssetCode = request.AssetCode?.Trim(),
                PurchasePrice = request.PurchasePrice,
                PurchaseDate = request.PurchaseDate,
                ExpectedDepreciationDate = request.ExpectedDepreciationDate,
                Location = request.Location?.Trim(),
                Note = request.Note?.Trim()
            };

            await _uow.Repository<F03EquipmentRequest>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            await _approvalSelections.ReplaceAsync(
                RequestModule.Equipment,
                entity.Id,
                request.ApprovalSelections,
                user.UserId,
                ct);

            return ServiceResult<EquipmentRequestDto>.Ok(
                await MapRequestAsync(entity, ct),
                "Đã tạo đăng ký thiết bị ở trạng thái nháp.");
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (KeyNotFoundException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (ArgumentException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<EquipmentRequestDto>> SubmitRegistrationAsync(int requestId, CancellationToken ct = default)
    {
        try
        {
            var entity = await GetOwnedRequestAsync(requestId, EquipmentRequestKind.Registration, ct);
            if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision)
                return ServiceResult<EquipmentRequestDto>.Fail("Chỉ đăng ký Nháp/NeedsRevision mới được gửi duyệt.");

            entity.RequestStatus = ApprovalStatus.Pending;
            await _uow.SaveChangesAsync(ct);

            var approvalResult = await InitApprovalAsync(entity, ct);
            if (!approvalResult.IsSuccess)
            {
                entity.RequestStatus = ApprovalStatus.Draft;
                await _uow.SaveChangesAsync(ct);
                return ServiceResult<EquipmentRequestDto>.Fail(
                    approvalResult.Message ?? "Không thể khởi tạo luồng duyệt thiết bị.");
            }

            return ServiceResult<EquipmentRequestDto>.Ok(
                await MapRequestAsync(entity, ct),
                "Đã gửi đăng ký thiết bị vào quy trình phê duyệt.");
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (KeyNotFoundException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (ArgumentException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<List<EquipmentRequestDto>>> GetMineAsync(
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireModuleUser();
            var scope = await _authorization.GetScopeAsync(
                user.UserId, SecurityFunctionCodes.EquipmentView, ct);

            var query = _uow.Repository<F03EquipmentRequest>()
                .Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true);

            if (scope != AuthorizationScopeCodes.All)
            {
                var managedEmployees =
                    await _authorization.GetManagedEmployeesAsync(user.UserId, ct);

                var managedEmployeeCodes = managedEmployees
                    .Select(x => x.EmployeeCode)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var managedDeptCodes = managedEmployees
                    .Select(x => x.DeptCode)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (scope == AuthorizationScopeCodes.Own ||
                    scope == AuthorizationScopeCodes.Employee)
                {
                    query = query.Where(x =>
                        x.EmployeeCode == user.EmployeeCode ||
                        managedEmployeeCodes.Contains(x.EmployeeCode));
                }
                else if (scope == AuthorizationScopeCodes.Department)
                {
                    query = query.Where(x =>
                        x.DeptCode == user.DeptCode ||
                        managedDeptCodes.Contains(x.DeptCode));
                }
                else
                {
                    query = query.Where(x => false);
                }
            }

            var rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            return ServiceResult<List<EquipmentRequestDto>>.Ok(
                rows.Select(x => MapRequest(x)).ToList());
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<EquipmentRequestDto>>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<EquipmentRequestDto>> CreateRepairDraftAsync(
        CreateEquipmentRepairDto request,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireModuleUser();
            var asset = await GetActiveAssetForUserAsync(request.AssetId, ct);

            if (string.IsNullOrWhiteSpace(request.RepairContent))
                return ServiceResult<EquipmentRequestDto>.Fail(
                    "Nội dung sửa chữa là bắt buộc.");

            if (request.ApprovalSelections == null ||
                request.ApprovalSelections.Count == 0)
                return ServiceResult<EquipmentRequestDto>.Fail(
                    "Vui lòng chọn người phê duyệt cho từng cấp.");

            var entity = new F03EquipmentRequest
            {
                RequestKind = EquipmentRequestKind.Repair,
                AssetId = asset.Id,
                EmployeeCode = user.EmployeeCode ?? string.Empty,
                DeptCode = asset.DeptCode,
                RequestStatus = ApprovalStatus.Draft,
                CreatedBy = user.UserId,
                OperatorUserId = user.UserId,
                SelectedApproverCode = request.ApprovalSelections
                    .OrderBy(x => x.Level)
                    .Select(x => x.ApproverCode)
                    .FirstOrDefault()
                    ?? request.SelectedApproverCode?.Trim()
                    ?? string.Empty,
                QrToken = asset.QrToken,
                EquipmentName = asset.EquipmentName,
                AssetCode = asset.AssetCode,
                PurchasePrice = asset.PurchasePrice,
                PurchaseDate = asset.PurchaseDate,
                ExpectedDepreciationDate = asset.ExpectedDepreciationDate,
                RepairDate = request.RepairDate,
                RepairContent = request.RepairContent.Trim(),
                RepairVendor = request.RepairVendor?.Trim(),
                RepairCost = request.RepairCost,
                RepairResult = request.RepairResult?.Trim(),
                Note = request.Note?.Trim()
            };

            await _uow.Repository<F03EquipmentRequest>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // Registration already persisted its selections. Repair must do the same;
            // otherwise submit-time snapshot creation cannot resolve the selected approvers.
            await _approvalSelections.ReplaceAsync(
                RequestModule.Equipment,
                entity.Id,
                request.ApprovalSelections,
                user.UserId,
                ct);

            return ServiceResult<EquipmentRequestDto>.Ok(
                await MapRequestAsync(entity, ct),
                "Đã tạo yêu cầu sửa chữa ở trạng thái nháp.");
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (KeyNotFoundException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (ArgumentException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<EquipmentRequestDto>> SubmitRepairAsync(int requestId, CancellationToken ct = default)
    {
        try
        {
            var entity = await GetOwnedRequestAsync(requestId, EquipmentRequestKind.Repair, ct);
            if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision)
                return ServiceResult<EquipmentRequestDto>.Fail("Chỉ repair Nháp/NeedsRevision mới được gửi duyệt.");

            entity.RequestStatus = ApprovalStatus.Pending;
            await _uow.SaveChangesAsync(ct);

            var approvalResult = await InitApprovalAsync(entity, ct);
            if (!approvalResult.IsSuccess)
            {
                entity.RequestStatus = ApprovalStatus.Draft;
                await _uow.SaveChangesAsync(ct);
                return ServiceResult<EquipmentRequestDto>.Fail(
                    approvalResult.Message ?? "Không thể khởi tạo luồng duyệt sửa chữa.");
            }

            return ServiceResult<EquipmentRequestDto>.Ok(
                await MapRequestAsync(entity, ct),
                "Đã gửi yêu cầu sửa chữa vào quy trình phê duyệt.");
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (KeyNotFoundException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
        catch (ArgumentException ex) { return ServiceResult<EquipmentRequestDto>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<EquipmentAssetDto>> ScanAsync(
        string qrToken,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireModuleUser();

            if (string.IsNullOrWhiteSpace(qrToken))
                return ServiceResult<EquipmentAssetDto>.Fail("QR token không hợp lệ.");

            var asset = await _uow.Repository<F03EquipmentAsset>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.QrToken == qrToken && x.IsActive == true, ct);

            if (asset == null || !asset.IsQrActive)
                return ServiceResult<EquipmentAssetDto>.Fail(
                    "QR chưa có hiệu lực hoặc thiết bị không tồn tại.");

            await EnsureScopeAsync(
                user, SecurityFunctionCodes.EquipmentQR,
                null, asset.DeptCode, ct);

            return ServiceResult<EquipmentAssetDto>.Ok(
                await MapAssetAsync(asset, ct));
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<EquipmentAssetDto>.Fail(ex.Message); }
        catch (ArgumentException ex) { return ServiceResult<EquipmentAssetDto>.Fail(ex.Message); }
    }
    public async Task<ServiceResult<EquipmentAssetDto?>> GetAssetAsync(
        int assetId,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireModuleUser();

            var asset = await _uow.Repository<F03EquipmentAsset>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == assetId &&
                         x.IsActive == true &&
                         x.IsQrActive,
                    ct);

            if (asset == null)
                return ServiceResult<EquipmentAssetDto?>.Ok(null);

            await EnsureScopeAsync(
                user, SecurityFunctionCodes.EquipmentView,
                null, asset.DeptCode, ct);

            return ServiceResult<EquipmentAssetDto?>.Ok(
                await MapAssetAsync(asset, ct));
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<EquipmentAssetDto?>.Fail(ex.Message); }
    }
    private async Task<ServiceResult> InitApprovalAsync(F03EquipmentRequest entity, CancellationToken ct)
    {
        var user = RequireUser();
        return await _workflow.InitApprovalAsync(
            entity.Id,
            ApprovalBuildContext.ForEquipment(
                entity.Id,
                entity.EmployeeCode,
                entity.DeptCode ?? string.Empty,
                user.PositionCode ?? string.Empty),
            ct);
    }
    private async Task<F03EquipmentRequest> GetOwnedRequestAsync(int id, EquipmentRequestKind kind, CancellationToken ct) { var user = RequireModuleUser(); var entity = await _uow.Repository<F03EquipmentRequest>().Query().FirstOrDefaultAsync(x => x.Id == id && x.RequestKind == kind && x.IsActive == true, ct) ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu thiết bị."); if (!await _authorization.CanAccessAsync(user, kind == EquipmentRequestKind.Repair ? SecurityFunctionCodes.EquipmentRepair : SecurityFunctionCodes.EquipmentEdit, entity.EmployeeCode, entity.DeptCode, ct)) throw new UnauthorizedAccessException("Bạn không có quyền thao tác yêu cầu này."); return entity; }
    private async Task<F03EquipmentAsset> GetActiveAssetForUserAsync(int id, CancellationToken ct)
    {
        var user = RequireModuleUser();
        var asset = await _uow.Repository<F03EquipmentAsset>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true && x.IsQrActive, ct)
            ?? throw new KeyNotFoundException("Thiết bị không tồn tại hoặc QR chưa có hiệu lực.");

        await EnsureScopeAsync(user, SecurityFunctionCodes.EquipmentRepair, user.EmployeeCode, asset.DeptCode, ct);
        return asset;
    }
    private async Task EnsureScopeAsync(UserIdentityDto user, int functionCode, string? employeeCode, string? deptCode, CancellationToken ct)
    {
        if (!await _authorization.CanAccessAsync(user, functionCode, employeeCode, deptCode, ct))
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập dữ liệu theo phạm vi được cấp.");
    }

    private async Task EnsureDepartmentScopeForOwnedDataAsync(UserIdentityDto user, string? deptCode, CancellationToken ct)
    {
        if (!string.Equals(user.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Bạn chỉ được tạo dữ liệu cho bộ phận của mình.");
        await Task.CompletedTask;
    }

    private UserIdentityDto RequireUser() => _currentUser.GetCurrentUser() ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
    private UserIdentityDto RequireModuleUser() => RequireUser();
    private static void ValidateRegistration(CreateEquipmentRegistrationDto x) { if (string.IsNullOrWhiteSpace(x.EquipmentName)) throw new ArgumentException("Tên thiết bị là bắt buộc."); if (x.PurchaseDate > DateTime.Today) throw new ArgumentException("Ngày mua không được lớn hơn ngày hiện tại."); if (x.ExpectedDepreciationDate < x.PurchaseDate) throw new ArgumentException("Ngày khấu hao dự kiến phải sau ngày mua."); if (x.PurchasePrice < 0) throw new ArgumentException("Giá thành không hợp lệ."); }
    private async Task<EquipmentRequestDto> MapRequestAsync(F03EquipmentRequest x, CancellationToken ct) { var name = await _uow.Repository<F03Employee>().Query().AsNoTracking().Where(e => e.EmployeeCode == x.EmployeeCode).Select(e => e.EmployeeName).FirstOrDefaultAsync(ct); return MapRequest(x, name); }
    private static EquipmentRequestDto MapRequest(F03EquipmentRequest x, string? employeeName = null) => new() { Id = x.Id, RequestKind = x.RequestKind, RequestStatus = x.RequestStatus, AssetId = x.AssetId, EmployeeCode = x.EmployeeCode, EmployeeName = employeeName, DeptCode = x.DeptCode ?? string.Empty, EquipmentName = x.EquipmentName, AssetCode = x.AssetCode, PurchasePrice = x.PurchasePrice, PurchaseDate = x.PurchaseDate, ExpectedDepreciationDate = x.ExpectedDepreciationDate, RepairDate = x.RepairDate, RepairContent = x.RepairContent, RepairCost = x.RepairCost, QrToken = x.QrToken, QrUrl = $"/equipment/scan/{x.QrToken}", SelectedApproverCode = x.SelectedApproverCode };
    private async Task<EquipmentAssetDto> MapAssetAsync(F03EquipmentAsset x, CancellationToken ct) { var history = await _uow.Repository<F03EquipmentRepairHistory>().Query().AsNoTracking().Where(h => h.AssetId == x.Id && h.IsApproved).OrderByDescending(h => h.RepairDate).ToListAsync(ct); return new EquipmentAssetDto { Id = x.Id, EquipmentCode = x.EquipmentCode, EquipmentName = x.EquipmentName, Specification = x.Specification, SerialNumber = x.SerialNumber, AssetCode = x.AssetCode, PurchasePrice = x.PurchasePrice, PurchaseDate = x.PurchaseDate, ExpectedDepreciationDate = x.ExpectedDepreciationDate, DeptCode = x.DeptCode, Location = x.Location, QrToken = x.QrToken, QrUrl = $"/equipment/scan/{x.QrToken}", IsQrActive = x.IsQrActive, Note = x.Note, RepairHistory = history.Select(h => new EquipmentRepairHistoryDto { Id = h.Id, RepairDate = h.RepairDate, OperatorUserId = h.OperatorUserId, RepairCost = h.RepairCost, RepairContent = h.RepairContent, RepairVendor = h.RepairVendor, RepairResult = h.RepairResult, Note = h.Note }).ToList() }; }
}
