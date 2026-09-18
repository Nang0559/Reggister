using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Equipment;
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
    private readonly IUnitOfWork _uow; private readonly ICurrentUserService _currentUser; private readonly IApprovalWorkflowOrchestrator<EquipmentRequestSubject> _workflow;
    public EquipmentService(IUnitOfWork uow, ICurrentUserService currentUser, IApprovalWorkflowOrchestrator<EquipmentRequestSubject> workflow) { _uow = uow; _currentUser = currentUser; _workflow = workflow; }
    public Task<bool> HasModuleAccessAsync(CancellationToken ct = default) { var user = RequireUser(); return Task.FromResult(user.IsAdmin || user.Functions.Contains(UserFunctionCodes.EquipmentModule)); }
    public async Task<List<EquipmentApproverDto>> GetApproversAsync(string deptCode, CancellationToken ct = default)
    { var user = RequireModuleUser(); if (string.IsNullOrWhiteSpace(deptCode)) throw new ArgumentException("Bộ phận là bắt buộc."); EnsureDepartmentScope(user, deptCode); return await _uow.Repository<F03Approver>().Query().AsNoTracking().Where(x => x.RequestType == RequestModule.Equipment && x.IsActive == true && (x.ApproveForDeptCode == deptCode || x.ApproveForDeptCode == ApproveForDept.All)).OrderBy(x => x.Level).ThenBy(x => x.ApproverName).Select(x => new EquipmentApproverDto { ApproverCode = x.ApproverCode, ApproverName = x.ApproverName, ApproverEmail = x.ApproverEmail, Level = x.Level, RoleName = x.RoleName, ApproveForDeptCode = x.ApproveForDeptCode }).ToListAsync(ct); }
    public async Task<EquipmentRequestDto> CreateRegistrationDraftAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default)
    { var user = RequireModuleUser(); ValidateRegistration(request); EnsureDepartmentScope(user, request.DeptCode); await ValidateSelectedApproverAsync(request.DeptCode, request.SelectedApproverCode, ct); var entity = new F03EquipmentRequest { RequestKind = EquipmentRequestKind.Registration, EmployeeCode = user.EmployeeCode ?? string.Empty, DeptCode = request.DeptCode, RequestStatus = ApprovalStatus.Draft, CreatedBy = user.UserId, OperatorUserId = user.UserId, SelectedApproverCode = request.SelectedApproverCode.Trim(), QrToken = Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Guid.NewGuid().ToString("N"), EquipmentName = request.EquipmentName.Trim(), Specification = request.Specification?.Trim(), SerialNumber = request.SerialNumber?.Trim(), AssetCode = request.AssetCode?.Trim(), PurchasePrice = request.PurchasePrice, PurchaseDate = request.PurchaseDate, ExpectedDepreciationDate = request.ExpectedDepreciationDate, Location = request.Location?.Trim(), Note = request.Note?.Trim() }; await _uow.Repository<F03EquipmentRequest>().AddAsync(entity, ct); await _uow.SaveChangesAsync(ct); return await MapRequestAsync(entity, ct); }
    public async Task<EquipmentRequestDto> SubmitRegistrationAsync(int requestId, CancellationToken ct = default)
    { var entity = await GetOwnedRequestAsync(requestId, EquipmentRequestKind.Registration, ct); if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision) throw new InvalidOperationException("Chỉ đăng ký Nháp/NeedsRevision mới được gửi duyệt."); await ValidateSelectedApproverAsync(entity.DeptCode, entity.SelectedApproverCode, ct); entity.RequestStatus = ApprovalStatus.Pending; await _uow.SaveChangesAsync(ct); await InitApprovalAsync(entity, ct); return await MapRequestAsync(entity, ct); }
    public async Task<List<EquipmentRequestDto>> GetMineAsync(CancellationToken ct = default)
    { var user = RequireModuleUser(); var rows = await _uow.Repository<F03EquipmentRequest>().Query().AsNoTracking().Where(x => x.IsActive == true && (user.IsAdmin || x.EmployeeCode == user.EmployeeCode)).OrderByDescending(x => x.CreatedAt).ToListAsync(ct); return rows.Select(x => MapRequest(x)).ToList(); }
    public async Task<EquipmentRequestDto> CreateRepairDraftAsync(CreateEquipmentRepairDto request, CancellationToken ct = default)
    { var user = RequireModuleUser(); var asset = await GetActiveAssetForUserAsync(request.AssetId, ct); if (string.IsNullOrWhiteSpace(request.RepairContent)) throw new ArgumentException("Nội dung sửa chữa là bắt buộc."); await ValidateSelectedApproverAsync(asset.DeptCode, request.SelectedApproverCode, ct); var entity = new F03EquipmentRequest { RequestKind = EquipmentRequestKind.Repair, AssetId = asset.Id, EmployeeCode = user.EmployeeCode ?? string.Empty, DeptCode = asset.DeptCode, RequestStatus = ApprovalStatus.Draft, CreatedBy = user.UserId, OperatorUserId = user.UserId, SelectedApproverCode = request.SelectedApproverCode.Trim(), QrToken = asset.QrToken, EquipmentName = asset.EquipmentName, AssetCode = asset.AssetCode, PurchasePrice = asset.PurchasePrice, PurchaseDate = asset.PurchaseDate, ExpectedDepreciationDate = asset.ExpectedDepreciationDate, RepairDate = request.RepairDate, RepairContent = request.RepairContent.Trim(), RepairVendor = request.RepairVendor?.Trim(), RepairCost = request.RepairCost, RepairResult = request.RepairResult?.Trim(), Note = request.Note?.Trim() }; await _uow.Repository<F03EquipmentRequest>().AddAsync(entity, ct); await _uow.SaveChangesAsync(ct); return await MapRequestAsync(entity, ct); }
    public async Task<EquipmentRequestDto> SubmitRepairAsync(int requestId, CancellationToken ct = default)
    { var entity = await GetOwnedRequestAsync(requestId, EquipmentRequestKind.Repair, ct); if (entity.RequestStatus != ApprovalStatus.Draft && entity.RequestStatus != ApprovalStatus.NeedsRevision) throw new InvalidOperationException("Chỉ repair Nháp/NeedsRevision mới được gửi duyệt."); await ValidateSelectedApproverAsync(entity.DeptCode, entity.SelectedApproverCode, ct); entity.RequestStatus = ApprovalStatus.Pending; await _uow.SaveChangesAsync(ct); await InitApprovalAsync(entity, ct); return await MapRequestAsync(entity, ct); }
    public async Task<EquipmentAssetDto> ScanAsync(string qrToken, CancellationToken ct = default)
    { var user = RequireModuleUser(); if (string.IsNullOrWhiteSpace(qrToken)) throw new ArgumentException("QR token không hợp lệ."); var asset = await _uow.Repository<F03EquipmentAsset>().Query().AsNoTracking().FirstOrDefaultAsync(x => x.QrToken == qrToken && x.IsActive == true, ct); if (asset == null || !asset.IsQrActive) throw new KeyNotFoundException("QR chưa có hiệu lực hoặc thiết bị không tồn tại."); EnsureDepartmentScope(user, asset.DeptCode); return await MapAssetAsync(asset, ct); }
    public async Task<EquipmentAssetDto?> GetAssetAsync(int assetId, CancellationToken ct = default)
    { var user = RequireModuleUser(); var asset = await _uow.Repository<F03EquipmentAsset>().Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == assetId && x.IsActive == true && x.IsQrActive, ct); if (asset == null) return null; EnsureDepartmentScope(user, asset.DeptCode); return await MapAssetAsync(asset, ct); }
    private async Task InitApprovalAsync(F03EquipmentRequest entity, CancellationToken ct) { var user = RequireUser(); await _workflow.InitApprovalAsync(entity.Id, ApprovalBuildContext.ForEquipment(entity.EmployeeCode, entity.DeptCode ?? string.Empty, user.PositionCode ?? string.Empty, entity.SelectedApproverCode), ct); }
    private async Task<F03EquipmentRequest> GetOwnedRequestAsync(int id, EquipmentRequestKind kind, CancellationToken ct) { var user = RequireModuleUser(); var entity = await _uow.Repository<F03EquipmentRequest>().Query().FirstOrDefaultAsync(x => x.Id == id && x.RequestKind == kind && x.IsActive == true, ct) ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu thiết bị."); if (!user.IsAdmin && !string.Equals(entity.EmployeeCode, user.EmployeeCode, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException("Bạn không có quyền thao tác yêu cầu này."); return entity; }
    private async Task<F03EquipmentAsset> GetActiveAssetForUserAsync(int id, CancellationToken ct)
    {
        var user = RequireModuleUser();
        var asset = await _uow.Repository<F03EquipmentAsset>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true && x.IsQrActive, ct)
            ?? throw new KeyNotFoundException("Thiết bị không tồn tại hoặc QR chưa có hiệu lực.");

        EnsureDepartmentScope(user, asset.DeptCode);
        return asset;
    }
    private async Task ValidateSelectedApproverAsync(string? deptCode, string code, CancellationToken ct) { if (string.IsNullOrWhiteSpace(deptCode) || string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Bộ phận và người phê duyệt là bắt buộc."); var ok = await _uow.Repository<F03Approver>().Query().AsNoTracking().AnyAsync(x => x.RequestType == RequestModule.Equipment && x.IsActive == true && x.ApproverCode == code && (x.ApproveForDeptCode == deptCode || x.ApproveForDeptCode == ApproveForDept.All), ct); if (!ok) throw new InvalidOperationException("Người phê duyệt không thuộc ma trận Equipment của bộ phận."); }
    private static void EnsureDepartmentScope(UserIdentityDto user, string? deptCode)
    {
        if (user.IsAdmin)
            return;

        if (string.IsNullOrWhiteSpace(user.DeptCode) ||
            !string.Equals(user.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem dữ liệu thiết bị của bộ phận này.");
        }
    }

    private UserIdentityDto RequireUser() => _currentUser.GetCurrentUser() ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
    private UserIdentityDto RequireModuleUser() { var user = RequireUser(); if (!user.IsAdmin && !user.Functions.Contains(UserFunctionCodes.EquipmentModule)) throw new UnauthorizedAccessException("Tài khoản chưa được cấp quyền sử dụng Sổ quản lý thiết bị."); return user; }
    private static void ValidateRegistration(CreateEquipmentRegistrationDto x) { if (string.IsNullOrWhiteSpace(x.EquipmentName)) throw new ArgumentException("Tên thiết bị là bắt buộc."); if (x.PurchaseDate > DateTime.Today) throw new ArgumentException("Ngày mua không được lớn hơn ngày hiện tại."); if (x.ExpectedDepreciationDate < x.PurchaseDate) throw new ArgumentException("Ngày khấu hao dự kiến phải sau ngày mua."); if (x.PurchasePrice < 0) throw new ArgumentException("Giá thành không hợp lệ."); }
    private async Task<EquipmentRequestDto> MapRequestAsync(F03EquipmentRequest x, CancellationToken ct) { var name = await _uow.Repository<F03Employee>().Query().AsNoTracking().Where(e => e.EmployeeCode == x.EmployeeCode).Select(e => e.EmployeeName).FirstOrDefaultAsync(ct); return MapRequest(x, name); }
    private static EquipmentRequestDto MapRequest(F03EquipmentRequest x, string? employeeName = null) => new() { Id = x.Id, RequestKind = x.RequestKind, RequestStatus = x.RequestStatus, AssetId = x.AssetId, EmployeeCode = x.EmployeeCode, EmployeeName = employeeName, DeptCode = x.DeptCode ?? string.Empty, EquipmentName = x.EquipmentName, AssetCode = x.AssetCode, PurchasePrice = x.PurchasePrice, PurchaseDate = x.PurchaseDate, ExpectedDepreciationDate = x.ExpectedDepreciationDate, RepairDate = x.RepairDate, RepairContent = x.RepairContent, RepairCost = x.RepairCost, QrToken = x.QrToken, QrUrl = $"/equipment/scan/{x.QrToken}", SelectedApproverCode = x.SelectedApproverCode };
    private async Task<EquipmentAssetDto> MapAssetAsync(F03EquipmentAsset x, CancellationToken ct) { var history = await _uow.Repository<F03EquipmentRepairHistory>().Query().AsNoTracking().Where(h => h.AssetId == x.Id && h.IsApproved).OrderByDescending(h => h.RepairDate).ToListAsync(ct); return new EquipmentAssetDto { Id = x.Id, EquipmentCode = x.EquipmentCode, EquipmentName = x.EquipmentName, Specification = x.Specification, SerialNumber = x.SerialNumber, AssetCode = x.AssetCode, PurchasePrice = x.PurchasePrice, PurchaseDate = x.PurchaseDate, ExpectedDepreciationDate = x.ExpectedDepreciationDate, DeptCode = x.DeptCode, Location = x.Location, QrToken = x.QrToken, QrUrl = $"/equipment/scan/{x.QrToken}", IsQrActive = x.IsQrActive, Note = x.Note, RepairHistory = history.Select(h => new EquipmentRepairHistoryDto { Id = h.Id, RepairDate = h.RepairDate, OperatorUserId = h.OperatorUserId, RepairCost = h.RepairCost, RepairContent = h.RepairContent, RepairVendor = h.RepairVendor, RepairResult = h.RepairResult, Note = h.Note }).ToList() }; }
}
