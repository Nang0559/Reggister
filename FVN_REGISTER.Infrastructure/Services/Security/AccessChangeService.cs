using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Models.Actions;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

public sealed class AccessChangeService : IAccessChangeService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;
    private readonly IApprovalRouteService _approvalRoute;
    private readonly IActionItemWriter _actionWriter;
    private readonly IEquipmentService _equipment;
    private readonly FVN_REGISTER.Application.Interfaces.Auths.ISessionService _sessions;

    public AccessChangeService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IAuthorizationService authorization,
        IApprovalRouteService approvalRoute,
        IActionItemWriter actionWriter,
        IEquipmentService equipment,
        FVN_REGISTER.Application.Interfaces.Auths.ISessionService sessions)
    {
        _uow = uow;
        _currentUser = currentUser;
        _authorization = authorization;
        _approvalRoute = approvalRoute;
        _actionWriter = actionWriter;
        _equipment = equipment;
        _sessions = sessions;
    }

    public async Task<ServiceResult<List<AccessChangeEmployeeOptionDto>>> GetEmployeeOptionsAsync(CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeCreate, ct))
                return ServiceResult<List<AccessChangeEmployeeOptionDto>>.Fail("Bạn chưa được cấp quyền lập phiếu thay đổi quyền.");

            var rows = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                .OrderBy(x => x.DeptCode).ThenBy(x => x.EmployeeName)
                .Select(x => new AccessChangeEmployeeOptionDto
                {
                    EmployeeCode = x.EmployeeCode,
                    EmployeeName = x.EmployeeName,
                    DeptCode = x.DeptCode,
                    PositionCode = x.PositionCode,
                    IsActive = x.IsActive == true
                })
                .ToListAsync(ct);

            return ServiceResult<List<AccessChangeEmployeeOptionDto>>.Ok(rows);
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<AccessChangeEmployeeOptionDto>>.Fail(ex.Message); }
    }

    public async Task<ServiceResult<List<AccessChangeFunctionOptionDto>>> GetFunctionOptionsAsync(
        RequestModule businessModule,
        string oldEmployeeCode,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeCreate, ct))
                return ServiceResult<List<AccessChangeFunctionOptionDto>>.Fail("Bạn chưa được cấp quyền lập phiếu thay đổi quyền.");

            if (businessModule is RequestModule.Attendance or RequestModule.AccessChange)
                return ServiceResult<List<AccessChangeFunctionOptionDto>>.Fail("Nghiệp vụ này chưa hỗ trợ bàn giao quyền.");

            var old = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == oldEmployeeCode.Trim(), ct);
            if (old == null)
                return ServiceResult<List<AccessChangeFunctionOptionDto>>.Fail("Không tìm thấy nhân sự cũ trong HRM.");

            var oldUser = await _uow.Repository<F03User>().Query().AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == old.EmployeeCode, ct);
            if (oldUser == null)
                return ServiceResult<List<AccessChangeFunctionOptionDto>>.Fail("Nhân sự cũ chưa có tài khoản FVN.");

            var snapshot = await _authorization.GetSnapshotAsync(oldUser.Id, ct);
            var moduleName = businessModule switch
            {
                RequestModule.Leave => "Leave",
                RequestModule.Overtime => "OT",
                RequestModule.Trip => "Trip",
                RequestModule.Equipment => "Equipment",
                _ => string.Empty
            };

            return ServiceResult<List<AccessChangeFunctionOptionDto>>.Ok(
                snapshot.Functions
                    .Where(x => string.Equals(x.ModuleCode, moduleName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new AccessChangeFunctionOptionDto
                    {
                        FunctionCode = x.FunctionCode,
                        FunctionName = x.FunctionName,
                        ModuleCode = x.ModuleCode,
                        ActionCode = x.ActionCode,
                        ScopeCode = x.ScopeCode
                    })
                    .ToList());
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<AccessChangeFunctionOptionDto>>.Fail(ex.Message); }
    }

    public async Task<ServiceResult<AccessChangeRequestDto>> CreateAndSubmitAsync(
        AccessChangeRequestCreateDto request,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeCreate, ct))
                return ServiceResult<AccessChangeRequestDto>.Fail("Bạn chưa được cấp quyền lập phiếu thay đổi quyền.");

            if (request.BusinessModule is RequestModule.Attendance or RequestModule.AccessChange)
                return ServiceResult<AccessChangeRequestDto>.Fail("Chỉ hỗ trợ Leave, OT, Trip và Equipment.");

            var requesterCode = user.EmployeeCode?.Trim();
            if (string.IsNullOrWhiteSpace(requesterCode))
                return ServiceResult<AccessChangeRequestDto>.Fail("Phiên đăng nhập chưa có EmployeeCode.");

            var requester = await _uow.Repository<F03Employee>().Query()
                .FirstOrDefaultAsync(x => x.IsActive == true && x.EmployeeCode == requesterCode, ct);
            if (requester == null)
                return ServiceResult<AccessChangeRequestDto>.Fail("Người lập phiếu phải là nhân sự HRM đang hoạt động.");

            var oldCode = request.OldEmployeeCode.Trim();
            if (string.IsNullOrWhiteSpace(oldCode) || oldCode.Equals(requesterCode, StringComparison.OrdinalIgnoreCase))
                return ServiceResult<AccessChangeRequestDto>.Fail("Nhân sự cũ không được trùng người lập phiếu.");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return ServiceResult<AccessChangeRequestDto>.Fail("Lý do thay đổi quyền là bắt buộc.");

            var oldEmployee = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == oldCode, ct);
            if (oldEmployee == null)
                return ServiceResult<AccessChangeRequestDto>.Fail("Không tìm thấy nhân sự cũ trong HRM.");

            var selectedFunctions = request.FunctionCodes.Distinct().ToList();
            var oldUser = await _uow.Repository<F03User>().Query().AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == oldCode, ct);

            if (oldUser != null)
            {
                var oldSnapshot = await _authorization.GetSnapshotAsync(oldUser.Id, ct);
                var moduleCode = ModuleCode(request.BusinessModule);
                var allowed = oldSnapshot.Functions
                    .Where(x => string.Equals(x.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.FunctionCode)
                    .ToHashSet();

                if (selectedFunctions.Count == 0)
                    selectedFunctions = oldSnapshot.Functions
                        .Where(x => string.Equals(x.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.FunctionCode)
                        .Distinct()
                        .ToList();
                else if (selectedFunctions.Any(x => !allowed.Contains(x)))
                    return ServiceResult<AccessChangeRequestDto>.Fail("Phiếu chỉ được chuyển các function mà nhân sự cũ đang có trong nghiệp vụ.");
            }
            else if (selectedFunctions.Count > 0)
            {
                return ServiceResult<AccessChangeRequestDto>.Fail("Nhân sự cũ chưa có tài khoản để xác định quyền cần bàn giao.");
            }

            var equipmentAssetIds = request.EquipmentAssetIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (request.BusinessModule == RequestModule.Equipment &&
                (request.TransferEquipmentResponsible || request.TransferEquipmentApprover))
            {
                if (equipmentAssetIds.Count == 0)
                    return ServiceResult<AccessChangeRequestDto>.Fail("Bàn giao thiết bị phải chọn ít nhất một tài sản.");

                var assets = await _uow.Repository<F03EquipmentAsset>().Query()
                    .AsNoTracking()
                    .Where(x => equipmentAssetIds.Contains(x.Id) && x.IsActive == true)
                    .Select(x => new
                    {
                        x.Id,
                        x.ResponsibleEmployeeCode,
                        x.ResponsibleApproverEmployeeCode
                    })
                    .ToListAsync(ct);

                if (assets.Count != equipmentAssetIds.Count)
                    return ServiceResult<AccessChangeRequestDto>.Fail("Danh sách tài sản bàn giao có tài sản không tồn tại hoặc đã ngừng hoạt động.");

                var oldApproverCode = request.TransferEquipmentApprover
                    ? request.OldEquipmentApproverCode?.Trim() ?? oldCode
                    : null;

                if (request.TransferEquipmentResponsible &&
                    assets.Any(x => !string.Equals(x.ResponsibleEmployeeCode, oldCode, StringComparison.OrdinalIgnoreCase)))
                    return ServiceResult<AccessChangeRequestDto>.Fail("Có tài sản không thuộc người phụ trách cũ nên không thể bàn giao trách nhiệm.");

                if (request.TransferEquipmentApprover &&
                    assets.Any(x => !string.Equals(x.ResponsibleApproverEmployeeCode, oldApproverCode, StringComparison.OrdinalIgnoreCase)))
                    return ServiceResult<AccessChangeRequestDto>.Fail("Có tài sản không thuộc approver cũ nên không thể bàn giao vai trò approver.");
            }

            var route = await _approvalRoute.GetPreviewAsync(request.BusinessModule, requesterCode, ct);
            if (!route.IsSuccess || route.Data == null)
                return ServiceResult<AccessChangeRequestDto>.Fail(
                    route.Message ?? "Chưa cấu hình luồng phê duyệt cho nghiệp vụ này.");

            var beforeSnapshot = await BuildPreChangeSnapshotAsync(
                request.BusinessModule, oldEmployee, selectedFunctions, request.EquipmentAssetIds, ct);

            var entity = new F03AccessChangeRequest
            {
                BusinessModule = request.BusinessModule,
                RequesterEmployeeCode = requesterCode,
                OldEmployeeCode = oldCode,
                NewEmployeeCode = requesterCode,
                DeptCode = requester.DeptCode ?? string.Empty,
                NewPositionCode = requester.PositionCode,
                Reason = request.Reason.Trim(),
                RequestedFunctionCodesJson = JsonSerializer.Serialize(selectedFunctions),
                EquipmentAssetIdsJson = JsonSerializer.Serialize(equipmentAssetIds),
                OldEquipmentResponsibleCode = request.TransferEquipmentResponsible ? oldCode : null,
                NewEquipmentResponsibleCode = request.TransferEquipmentResponsible ? requesterCode : null,
                OldEquipmentApproverCode = request.TransferEquipmentApprover
                    ? request.OldEquipmentApproverCode?.Trim() ?? oldCode
                    : null,
                NewEquipmentApproverCode = request.TransferEquipmentApprover ? requesterCode : null,
                Status = AccessChangeStatus.Pending,
                SubmittedAt = DateTime.Now,
                CreatedBy = user.UserId,
                LastModifiedSource = "ACCESS_CHANGE_REQUEST",
                PreChangeSnapshotJson = beforeSnapshot
            };

            await _uow.Repository<F03AccessChangeRequest>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var snapshotSteps = route.Data.Levels
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Level)
                .Select(level =>
                {
                    var candidate = level.Candidates.FirstOrDefault();
                    return new F03ApprovalStepSnapshot
                    {
                        Level = level.Level,
                        ApproverCode = candidate?.ApproverCode ?? string.Empty,
                        ApproverName = candidate?.ApproverName ?? string.Empty,
                        ApproverEmail = candidate?.ApproverEmail ?? string.Empty,
                        RoleName = level.RoleName,
                        IsRequired = level.Required
                    };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.ApproverCode) || !x.IsRequired)
                .ToList();

            if (snapshotSteps.Any(x => x.IsRequired && string.IsNullOrWhiteSpace(x.ApproverCode)))
            {
                entity.Status = AccessChangeStatus.Draft;
                await _uow.SaveChangesAsync(ct);
                return ServiceResult<AccessChangeRequestDto>.Fail("Không xác định được người duyệt cho một hoặc nhiều cấp.");
            }

            var snapshot = new F03ApprovalSnapshot
            {
                RequestId = entity.Id,
                RequestType = RequestModule.AccessChange,
                CreatedAt = DateTime.Now,
                Steps = snapshotSteps
            };
            await _uow.Repository<F03ApprovalSnapshot>().AddAsync(snapshot, ct);
            await _uow.SaveChangesAsync(ct);

            await CreateApproverActionAsync(entity, snapshotSteps.OrderBy(x => x.Level).FirstOrDefault(x => x.IsRequired), ct);
            return ServiceResult<AccessChangeRequestDto>.Ok(await MapAsync(entity.Id, ct), "Đã lập phiếu và gửi vào luồng phê duyệt.");
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or KeyNotFoundException)
        {
            return ServiceResult<AccessChangeRequestDto>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<AccessChangeRequestDto>> GetAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            var entity = await _uow.Repository<F03AccessChangeRequest>().Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true, ct);
            if (entity == null) return ServiceResult<AccessChangeRequestDto>.Fail("Không tìm thấy phiếu.");

            if (!await CanViewAsync(user, entity, ct))
                return ServiceResult<AccessChangeRequestDto>.Fail("Bạn không có quyền xem phiếu này.");

            return ServiceResult<AccessChangeRequestDto>.Ok(await MapAsync(id, ct));
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<AccessChangeRequestDto>.Fail(ex.Message); }
    }

    public async Task<ServiceResult<List<AccessChangeRequestDto>>> GetMineAsync(CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeView, ct))
                return ServiceResult<List<AccessChangeRequestDto>>.Fail("Bạn chưa được cấp quyền xem phiếu thay đổi quyền.");

            var code = user.EmployeeCode ?? string.Empty;
            var rows = await _uow.Repository<F03AccessChangeRequest>().Query()
                .Where(x => x.IsActive == true && x.RequesterEmployeeCode == code)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .ToListAsync(ct);

            var result = new List<AccessChangeRequestDto>();
            foreach (var row in rows) result.Add(await MapAsync(row.Id, ct));
            return ServiceResult<List<AccessChangeRequestDto>>.Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<AccessChangeRequestDto>>.Fail(ex.Message); }
    }

    public async Task<ServiceResult<AccessChangeRequestDto>> ApproveAsync(int id, int level, string? comment, CancellationToken ct = default)
        => await DecideAsync(id, level, comment, false, ct);

    public async Task<ServiceResult<AccessChangeRequestDto>> RejectAsync(int id, int level, string comment, CancellationToken ct = default)
        => await DecideAsync(id, level, comment, true, ct);

    public async Task<ServiceResult<List<AccessChangeRequestDto>>> GetPendingApprovalsAsync(CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeApprove, ct))
                return ServiceResult<List<AccessChangeRequestDto>>.Fail("Bạn chưa được cấp quyền phê duyệt.");

            var code = user.EmployeeCode ?? string.Empty;
            var snapshots = await _uow.Repository<F03ApprovalSnapshot>().Query()
                .Include(x => x.Steps)
                .Where(x => x.RequestType == RequestModule.AccessChange &&
                            x.Steps.Any(s => s.ApproverCode == code && s.IsRequired))
                .ToListAsync(ct);

            var result = new List<AccessChangeRequestDto>();
            foreach (var snapshot in snapshots)
            {
                var step = snapshot.Steps.FirstOrDefault(x => x.ApproverCode == code && x.IsRequired);
                if (step == null) continue;

                var histories = await _uow.Repository<F03ApprovalHistory>().Query()
                    .Where(x => x.RequestType == RequestModule.AccessChange && x.RequestId == snapshot.RequestId)
                    .ToListAsync(ct);

                if (histories.Any(x => x.StepId == step.Id)) continue;
                if (histories.Any(x => snapshot.Steps.Where(s => s.IsRequired && s.Level < step.Level).Select(s => s.Id).Contains(x.StepId)
                                     && x.Decision != DecisionType.Approved))
                    continue;
                var previousRequired = snapshot.Steps.Where(s => s.IsRequired && s.Level < step.Level).Select(s => s.Id).ToList();
                if (previousRequired.Any(id => !histories.Any(h => h.StepId == id && h.Decision == DecisionType.Approved)))
                    continue;

                var request = await _uow.Repository<F03AccessChangeRequest>().Query()
                    .FirstOrDefaultAsync(x => x.Id == snapshot.RequestId && x.IsActive == true &&
                        (x.Status == AccessChangeStatus.Pending || x.Status == AccessChangeStatus.InProgress), ct);
                if (request == null) continue;

                result.Add(await MapAsync(request.Id, ct));
            }
            return ServiceResult<List<AccessChangeRequestDto>>.Ok(result.OrderBy(x => x.SubmittedAt).ToList());
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<AccessChangeRequestDto>>.Fail(ex.Message); }
    }

    public async Task<ServiceResult<List<AccessChangeItQueueItemDto>>> GetItQueueAsync(CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeExecute, ct))
                return ServiceResult<List<AccessChangeItQueueItemDto>>.Fail("Bạn chưa được cấp quyền IT xử lý thay đổi quyền.");

            var rows = await _uow.Repository<F03AccessChangeRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true && x.Status == AccessChangeStatus.Approved)
                .OrderBy(x => x.SubmittedAt)
                .Take(200)
                .ToListAsync(ct);

            var result = new List<AccessChangeItQueueItemDto>();
            foreach (var row in rows)
            {
                var employeeName = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                    .Where(x => x.EmployeeCode == row.NewEmployeeCode)
                    .Select(x => x.EmployeeName)
                    .FirstOrDefaultAsync(ct);

                result.Add(new AccessChangeItQueueItemDto
                {
                    Id = row.Id,
                    BusinessModule = row.BusinessModule,
                    BusinessModuleName = DisplayModule(row.BusinessModule),
                    OldEmployeeCode = row.OldEmployeeCode,
                    NewEmployeeCode = row.NewEmployeeCode,
                    NewEmployeeName = employeeName ?? row.NewEmployeeCode,
                    DeptCode = row.DeptCode,
                    FunctionCodes = DeserializeInts(row.RequestedFunctionCodesJson),
                    TransferEquipmentResponsible = !string.IsNullOrWhiteSpace(row.OldEquipmentResponsibleCode),
                    TransferEquipmentApprover = !string.IsNullOrWhiteSpace(row.OldEquipmentApproverCode),
                    Status = row.Status,
                    SubmittedAt = row.SubmittedAt ?? row.CreatedAt
                });
            }
            return ServiceResult<List<AccessChangeItQueueItemDto>>.Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<List<AccessChangeItQueueItemDto>>.Fail(ex.Message); }
    }

    public async Task<ServiceResult<AccessChangeRequestDto>> ExecuteByItAsync(
        int id,
        AccessChangeItExecuteDto request,
        CancellationToken ct = default)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeExecute, ct))
                return ServiceResult<AccessChangeRequestDto>.Fail("Bạn chưa được cấp quyền IT xử lý thay đổi quyền.");

            var entity = await _uow.Repository<F03AccessChangeRequest>().Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true, ct);
            if (entity == null) return ServiceResult<AccessChangeRequestDto>.Fail("Không tìm thấy phiếu.");
            if (entity.Status != AccessChangeStatus.Approved)
                return ServiceResult<AccessChangeRequestDto>.Fail("Chỉ phiếu đã được phê duyệt đầy đủ mới được IT thực hiện.");

            var newUser = await _uow.Repository<F03User>().Query()
                .FirstOrDefaultAsync(x => x.EmployeeCode == entity.NewEmployeeCode, ct);
            if (newUser == null || newUser.IsActive != true)
                return ServiceResult<AccessChangeRequestDto>.Fail("Tài khoản người tiếp nhận chưa hoạt động.");

            var permission = await _uow.Repository<F03Permission>().Query()
                .FirstOrDefaultAsync(x => x.PermissionCode == newUser.PermissionCode, ct);
            if (permission == null)
                return ServiceResult<AccessChangeRequestDto>.Fail("Không xác định được Permission legacy của tài khoản người tiếp nhận.");

            var functionCodes = DeserializeInts(entity.RequestedFunctionCodesJson);
            var functions = await _uow.Repository<F03Function>().Query()
                .Where(x => functionCodes.Contains(x.FunctionCode) && (x.IsActive ?? true))
                .ToListAsync(ct);

            if (functions.Count != functionCodes.Distinct().Count())
                return ServiceResult<AccessChangeRequestDto>.Fail("Một hoặc nhiều function trong snapshot không còn hoạt động.");

            var userFunctionRepo = _uow.Repository<F03UserFunction>();
            foreach (var function in functions)
            {
                var exists = await userFunctionRepo.Query().AnyAsync(
                    x => x.IdUser == newUser.Id && x.IdFunction == function.Id, ct);
                if (!exists)
                {
                    await userFunctionRepo.AddAsync(new F03UserFunction
                    {
                        IdUser = newUser.Id,
                        IdPermission = permission.Id,
                        IdFunction = function.Id,
                        CreatedBy = user.UserId,
                        LastModifiedSource = "ACCESS_CHANGE_IT"
                    }, ct);
                }
            }

            if (entity.BusinessModule == RequestModule.Equipment &&
                (!string.IsNullOrWhiteSpace(entity.OldEquipmentResponsibleCode) ||
                 !string.IsNullOrWhiteSpace(entity.OldEquipmentApproverCode)))
            {
                var handover = await _equipment.HandoverAsync(new EquipmentHandoverRequest
                {
                    OldResponsibleEmployeeCode = entity.OldEquipmentResponsibleCode,
                    NewResponsibleEmployeeCode = entity.NewEquipmentResponsibleCode ?? entity.NewEmployeeCode,
                    OldApproverEmployeeCode = entity.OldEquipmentApproverCode,
                    NewApproverEmployeeCode = entity.NewEquipmentApproverCode ?? entity.NewEmployeeCode,
                    DeptCode = entity.DeptCode,
                    AssetIds = DeserializeInts(entity.EquipmentAssetIdsJson),
                    Reason = $"IT thực hiện theo phiếu thay đổi quyền #{entity.Id}: {entity.Reason}"
                }, ct);

                if (!handover.IsSuccess)
                    return ServiceResult<AccessChangeRequestDto>.Fail(
                        handover.Message ?? "Không thể bàn giao dữ liệu thiết bị theo phiếu.");
            }

            var oldUser = await _uow.Repository<F03User>().Query()
                .FirstOrDefaultAsync(x => x.EmployeeCode == entity.OldEmployeeCode, ct);
            var oldEmployee = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                .Where(x => x.EmployeeCode == entity.OldEmployeeCode)
                .Select(x => new { x.IsActive })
                .FirstOrDefaultAsync(ct);
            if (oldUser != null && oldEmployee?.IsActive != true)
            {
                oldUser.IsActive = false;
                oldUser.LockoutEndDate = DateTime.MaxValue;
                oldUser.ModifiedBy = user.UserId;
                oldUser.ModifiedAt = DateTime.Now;
                oldUser.LastModifiedSource = "ACCESS_CHANGE_IT_OLD_USER";
            }

            entity.Status = AccessChangeStatus.ITCompleted;
            entity.ITCompletedAt = DateTime.Now;
            entity.ITCompletedByUserId = user.UserId;
            entity.ITNote = request.Note?.Trim();
            entity.PostChangeResultJson = JsonSerializer.Serialize(new
            {
                CompletedAt = entity.ITCompletedAt,
                CompletedByUserId = entity.ITCompletedByUserId,
                BusinessModule = entity.BusinessModule,
                NewEmployeeCode = entity.NewEmployeeCode,
                EquipmentAssetIds = DeserializeInts(entity.EquipmentAssetIdsJson)
            });
            entity.ModifiedBy = user.UserId;
            entity.LastModifiedSource = "ACCESS_CHANGE_IT";
            await _uow.SaveChangesAsync(ct);

            await _sessions.RevokeAllAsync(newUser.Id, ct);
            var predecessorUserId = await _uow.Repository<F03User>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == entity.OldEmployeeCode && x.IsActive == false)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);
            if (predecessorUserId.HasValue)
                await _sessions.RevokeAllAsync(predecessorUserId.Value, ct);
            return ServiceResult<AccessChangeRequestDto>.Ok(await MapAsync(entity.Id, ct), "IT đã thiết lập quyền và hoàn tất phiếu.");
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<AccessChangeRequestDto>.Fail(ex.Message); }
    }

    private async Task<ServiceResult<AccessChangeRequestDto>> DecideAsync(
        int id,
        int level,
        string? comment,
        bool reject,
        CancellationToken ct)
    {
        try
        {
            var user = RequireUser();
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeApprove, ct))
                return ServiceResult<AccessChangeRequestDto>.Fail("Bạn chưa được cấp quyền phê duyệt phiếu thay đổi quyền.");

            var entity = await _uow.Repository<F03AccessChangeRequest>().Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true, ct);
            if (entity == null) return ServiceResult<AccessChangeRequestDto>.Fail("Không tìm thấy phiếu.");
            if (entity.Status is not (AccessChangeStatus.Pending or AccessChangeStatus.InProgress))
                return ServiceResult<AccessChangeRequestDto>.Fail("Phiếu không còn ở trạng thái chờ duyệt.");

            var snapshot = await _uow.Repository<F03ApprovalSnapshot>().Query()
                .Include(x => x.Steps)
                .FirstOrDefaultAsync(x => x.RequestId == id && x.RequestType == RequestModule.AccessChange, ct);
            if (snapshot == null) return ServiceResult<AccessChangeRequestDto>.Fail("Phiếu chưa có snapshot phê duyệt.");

            var step = snapshot.Steps.FirstOrDefault(x => x.Level == level && x.IsRequired);
            if (step == null) return ServiceResult<AccessChangeRequestDto>.Fail("Không tìm thấy cấp phê duyệt.");

            var approverCode = user.EmployeeCode ?? string.Empty;
            if (!string.Equals(step.ApproverCode, approverCode, StringComparison.OrdinalIgnoreCase) &&
                !user.Permission.IsAdmin())
                return ServiceResult<AccessChangeRequestDto>.Fail("Bạn không phải người được chỉ định ở cấp phê duyệt này.");

            var previousRequired = snapshot.Steps
                .Where(x => x.IsRequired && x.Level < level)
                .Select(x => x.Id)
                .ToList();
            var previousApproved = previousRequired.Count == 0 || await _uow.Repository<F03ApprovalHistory>().Query()
                .Where(x => x.RequestType == RequestModule.AccessChange && x.RequestId == id)
                .Where(x => previousRequired.Contains(x.StepId) && x.Decision == DecisionType.Approved)
                .CountAsync(ct) == previousRequired.Count;

            if (!previousApproved)
                return ServiceResult<AccessChangeRequestDto>.Fail("Cấp phê duyệt trước chưa hoàn tất.");

            var already = await _uow.Repository<F03ApprovalHistory>().Query()
                .AnyAsync(x => x.RequestType == RequestModule.AccessChange && x.RequestId == id && x.StepId == step.Id, ct);
            if (already)
                return ServiceResult<AccessChangeRequestDto>.Fail("Cấp phê duyệt này đã được xử lý.");

            await _uow.Repository<F03ApprovalHistory>().AddAsync(new F03ApprovalHistory
            {
                RequestType = RequestModule.AccessChange,
                RequestId = id,
                StepId = step.Id,
                ApproverCode = approverCode,
                ApproverName = step.ApproverName,
                Decision = reject ? DecisionType.Rejected : DecisionType.Approved,
                Comment = comment?.Trim(),
                ActionAt = DateTime.Now,
                IsOverriddenByAdmin = !string.Equals(step.ApproverCode, approverCode, StringComparison.OrdinalIgnoreCase)
            }, ct);

            if (reject)
                entity.Status = AccessChangeStatus.Rejected;
            else
                entity.Status = AccessChangeStatus.InProgress;

            await _uow.SaveChangesAsync(ct);
            await CompleteApproverActionAsync(id, level, approverCode, ct);

            if (!reject)
            {
                var next = await GetNextStepAsync(snapshot, id, ct);
                if (next != null)
                    await CreateApproverActionAsync(entity, next, ct);
                else
                    entity.Status = AccessChangeStatus.Approved;
            }

            await _uow.SaveChangesAsync(ct);
            return ServiceResult<AccessChangeRequestDto>.Ok(
                await MapAsync(id, ct),
                reject ? "Đã từ chối phiếu." : entity.Status == AccessChangeStatus.Approved ? "Đã phê duyệt cấp cuối." : "Đã phê duyệt cấp.");
        }
        catch (UnauthorizedAccessException ex) { return ServiceResult<AccessChangeRequestDto>.Fail(ex.Message); }
    }

    private async Task<F03ApprovalStepSnapshot?> GetNextStepAsync(
        F03ApprovalSnapshot snapshot, int requestId, CancellationToken ct)
    {
        var histories = await _uow.Repository<F03ApprovalHistory>().Query()
            .Where(x => x.RequestType == RequestModule.AccessChange && x.RequestId == requestId)
            .ToListAsync(ct);

        return snapshot.Steps
            .Where(x => x.IsRequired && histories.All(h => h.StepId != x.Id))
            .OrderBy(x => x.Level)
            .FirstOrDefault();
    }

    private async Task CreateApproverActionAsync(
        F03AccessChangeRequest request,
        F03ApprovalStepSnapshot? step,
        CancellationToken ct)
    {
        if (step == null || string.IsNullOrWhiteSpace(step.ApproverCode))
            return;

        var employee = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == step.ApproverCode)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync(ct);
        if (employee == null) return;

        var targetUserId = await _uow.Repository<F03User>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == step.ApproverCode && x.IsActive == true)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        await _actionWriter.EnsureOpenAsync(new ActionItemDraft(
            ModuleCode: "SecurityAccess",
            SourceId: request.Id.ToString(),
            EmployeeId: employee.Id,
            AssignedToEmployeeId: employee.Id,
            AssignedToUserId: targetUserId,
            WorkDate: null,
            ActionType: "APPROVAL",
            Title: $"Phê duyệt thay đổi quyền #{request.Id}",
            Summary: $"{DisplayModule(request.BusinessModule)}: {request.OldEmployeeCode} → {request.NewEmployeeCode}",
            Severity: 2,
            Priority: 50,
            DueAt: DateTime.Now.AddDays(2),
            DetailRoute: $"/security/access-change/{request.Id}",
            ReferenceNo: $"ACCESS-{request.Id}",
            PayloadJson: JsonSerializer.Serialize(new { request.Id, request.BusinessModule, Level = step.Level }),
            ActorUserId: request.CreatedBy), ct);
    }

    private async Task CompleteApproverActionAsync(int requestId, int level, string employeeCode, CancellationToken ct)
    {
        var employeeId = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking().Where(x => x.EmployeeCode == employeeCode).Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (employeeId <= 0) return;

        var rows = await _uow.Repository<F03ActionItem>().Query()
            .Where(x => x.ModuleCode == "SecurityAccess"
                     && x.SourceType == "MODULE"
                     && x.SourceId == requestId.ToString()
                     && x.ActionType == "APPROVAL"
                     && x.AssignedToEmployeeId == employeeId
                     && x.Status != ActionItemStatus.Completed
                     && x.Status != ActionItemStatus.Cancelled)
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            row.Status = ActionItemStatus.Completed;
            row.CompletedAt = DateTime.Now;
        }
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<string> BuildPreChangeSnapshotAsync(RequestModule module, F03Employee oldEmployee, IReadOnlyCollection<int> functionCodes, IReadOnlyCollection<int> assetIds, CancellationToken ct)
    {
        var snapshot = new
        {
            CapturedAt = DateTime.Now,
            BusinessModule = module.ToString(),
            OldEmployee = new { oldEmployee.EmployeeCode, oldEmployee.EmployeeName, oldEmployee.DeptCode, oldEmployee.PositionCode, oldEmployee.IsActive, oldEmployee.EndWorkingDate },
            FunctionCodes = functionCodes.Distinct().OrderBy(x => x).ToList(),
            Equipment = module == RequestModule.Equipment
                ? await BuildEquipmentPreChangeSnapshotAsync(assetIds, ct)
                : null
        };
        return JsonSerializer.Serialize(snapshot);
    }

    private async Task<object> BuildEquipmentPreChangeSnapshotAsync(IReadOnlyCollection<int> assetIds, CancellationToken ct)
    {
        var assets = await _uow.Repository<F03EquipmentAsset>().Query().AsNoTracking()
            .Where(x => assetIds.Contains(x.Id) && x.IsActive == true)
            .Select(x => new
            {
                x.Id, x.EquipmentCode, x.EquipmentName, x.AssetCode, x.SerialNumber, x.Specification,
                x.DeptCode, x.Location, x.PurchasePrice, x.ResponsibleEmployeeCode,
                x.ResponsibleApproverEmployeeCode, x.OperatingResponsibleDeptCode, x.OperatingResponsibleEmployeeCode,
                Repairs = x.RepairHistory.Where(h => h.IsApproved).OrderByDescending(h => h.RepairDate).Select(h => new
                {
                    h.Id, h.RequestId, h.RepairDate, h.RepairContent, h.RepairVendor, h.RepairCost,
                    h.RepairResult, h.ResponsibleDeptCode, h.RepairerEmployeeCode, h.RepairFeedback, h.CompletedAt
                }).ToList(),
                Handovers = x.AssignmentHistory.OrderByDescending(h => h.HandoverAt).Select(h => new
                {
                    h.Id, h.HandoverAt, h.PreviousResponsibleEmployeeCode, h.NewResponsibleEmployeeCode,
                    h.PreviousApproverEmployeeCode, h.NewApproverEmployeeCode, h.Reason
                }).ToList(),
                Checklists = x.InspectionTasks.OrderByDescending(t => t.ScheduledDate).Take(20).Select(t => new
                {
                    t.Id, t.TemplateId, t.ScheduledDate, t.DueAt, t.Status, t.Result,
                    t.InspectorEmployeeCode, t.ApproverEmployeeCode,
                    Results = t.ItemResults.Select(r => new { r.ItemId, r.ValueText, r.ValueNumber, r.Passed, r.Note }).ToList(),
                    Evidence = t.Evidence.Select(e => new { e.Id, e.FileName, e.ContentType, e.FileSize, e.StoragePath }).ToList()
                }).ToList()
            }).ToListAsync(ct);
        return assets;
    }

    private async Task<bool> CanViewAsync(UserIdentityDto user, F03AccessChangeRequest entity, CancellationToken ct)
    {
        if (string.Equals(user.EmployeeCode, entity.RequesterEmployeeCode, StringComparison.OrdinalIgnoreCase))
            return true;

        if (await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeApprove, ct))
            return true;

        return await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAccessChangeExecute, ct);
    }

    private async Task<AccessChangeRequestDto> MapAsync(int id, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03AccessChangeRequest>().Query().AsNoTracking()
            .FirstAsync(x => x.Id == id, ct);
        var employee = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => x.EmployeeCode == entity.NewEmployeeCode)
            .Select(x => new { x.EmployeeName, x.PositionCode })
            .FirstOrDefaultAsync(ct);

        var snapshot = await _uow.Repository<F03ApprovalSnapshot>().Query().AsNoTracking()
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.RequestId == id && x.RequestType == RequestModule.AccessChange, ct);
        var histories = await _uow.Repository<F03ApprovalHistory>().Query().AsNoTracking()
            .Where(x => x.RequestId == id && x.RequestType == RequestModule.AccessChange)
            .ToListAsync(ct);

        return new AccessChangeRequestDto
        {
            Id = entity.Id,
            BusinessModule = entity.BusinessModule,
            BusinessModuleName = DisplayModule(entity.BusinessModule),
            RequesterEmployeeCode = entity.RequesterEmployeeCode,
            OldEmployeeCode = entity.OldEmployeeCode,
            NewEmployeeCode = entity.NewEmployeeCode,
            DeptCode = entity.DeptCode,
            NewPositionCode = employee?.PositionCode ?? entity.NewPositionCode,
            Reason = entity.Reason,
            FunctionCodes = DeserializeInts(entity.RequestedFunctionCodesJson),
            EquipmentAssetIds = DeserializeInts(entity.EquipmentAssetIdsJson),
            TransferEquipmentResponsible = !string.IsNullOrWhiteSpace(entity.OldEquipmentResponsibleCode),
            TransferEquipmentApprover = !string.IsNullOrWhiteSpace(entity.OldEquipmentApproverCode),
            Status = entity.Status,
            ITNote = entity.ITNote,
            SubmittedAt = entity.SubmittedAt,
            ITCompletedAt = entity.ITCompletedAt,
            PreChangeSnapshotJson = entity.PreChangeSnapshotJson,
            PostChangeResultJson = entity.PostChangeResultJson,
            ApprovalSteps = snapshot?.Steps
                .OrderBy(x => x.Level)
                .Select(step =>
                {
                    var history = histories.FirstOrDefault(h => h.StepId == step.Id);
                    return new AccessChangeApprovalStepDto
                    {
                        Level = step.Level,
                        RoleName = step.RoleName,
                        ApproverCode = step.ApproverCode,
                        ApproverName = step.ApproverName,
                        Decision = history?.Decision.ToString() ?? DecisionType.Pending.ToString(),
                        ActionAt = history?.ActionAt
                    };
                }).ToList() ?? new()
        };
    }

    private UserIdentityDto RequireUser()
        => _currentUser.GetCurrentUser()
           ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");

    private static string ModuleCode(RequestModule module) => module switch
    {
        RequestModule.Leave => "Leave",
        RequestModule.Overtime => "OT",
        RequestModule.Trip => "Trip",
        RequestModule.Equipment => "Equipment",
        _ => string.Empty
    };

    private static string DisplayModule(RequestModule module) => module switch
    {
        RequestModule.Leave => "Nghỉ phép",
        RequestModule.Overtime => "Tăng ca",
        RequestModule.Trip => "Công tác",
        RequestModule.Equipment => "Thiết bị",
        _ => module.ToString()
    };

    private static List<int> DeserializeInts(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<int>>(json) ?? new(); }
        catch { return new(); }
    }
}
