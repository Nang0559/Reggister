using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Rules;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public class ApproverManagementService : BaseService<ApproverManagementService>, IApproverManagementService
{
    private readonly IUnitOfWork _uow;

    public ApproverManagementService(
        IUnitOfWork uow,
        ILogger<ApproverManagementService> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _uow = uow;
    }

    public async Task<ServiceResult<List<ApproverTreeNodeDto>>> GetApproverTreeAsync(CancellationToken ct)
    {
        try
        {
            var all = await _uow.Repository<F03Approver>().Query().AsNoTracking()
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.ApproveForDeptCode).ThenBy(x => x.Level)
                .ThenBy(x => x.RequestType).ThenBy(x => x.ApproverName)
                .ToListAsync(ct);

            var deptCodes = all.Select(x => x.ApproveForDeptCode)
                .Where(x => x != ApproveForDept.All).Distinct().ToList();

            var deptNames = await _uow.Repository<F03Department>().Query().AsNoTracking()
                .Where(x => deptCodes.Contains(x.DeptCode))
                .ToDictionaryAsync(x => x.DeptCode, x => x.DeptName, ct);

            var tree = all.GroupBy(x => x.ApproveForDeptCode)
                .Select(deptGroup => new ApproverTreeNodeDto
                {
                    DeptCode = deptGroup.Key,
                    DeptName = deptGroup.Key == ApproveForDept.All
                        ? "Toàn công ty (ALL)"
                        : deptNames.GetValueOrDefault(deptGroup.Key, deptGroup.Key),
                    Levels = deptGroup.GroupBy(x => x.Level).OrderBy(g => g.Key)
                        .Select(levelGroup => new ApproverTreeLevelGroupDto
                        {
                            Level = levelGroup.Key,
                            RoleName = RoleNameFromLevel(levelGroup.Key, levelGroup.First().RequestType),
                            LevelDisplayName = LevelDisplayName(levelGroup.Key),
                            Types = levelGroup.GroupBy(x => x.RequestType)
                                .Select(rtGroup => new ApproverTreeTypeGroupDto
                                {
                                    RequestType = rtGroup.Key,
                                    RequestTypeDisplay = rtGroup.Key.ToDisplayName(),
                                    Approvers = rtGroup.Select(ApproverMapper.ToDto).ToList()
                                })
                                .OrderBy(x => x.RequestType).ToList()
                        }).ToList()
                })
                .OrderBy(x => x.DeptCode == ApproveForDept.All ? "ZZZ" : x.DeptCode)
                .ToList();

            Logger.LogDebugIf(Debug, "[APPROVER-MGT] Tree: {DeptCount} groups", tree.Count);
            return ServiceResult<List<ApproverTreeNodeDto>>.Ok(tree);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] GetApproverTree ERROR");
            return ServiceResult<List<ApproverTreeNodeDto>>.Fail("Lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult<List<ApproverDto>>> GetListAsync(
        string? deptCode, int? level, RequestModule? requestType, CancellationToken ct)
    {
        try
        {
            var q = _uow.Repository<F03Approver>().Query().AsNoTracking()
                .Where(x => x.IsActive == true);

            if (!string.IsNullOrEmpty(deptCode))
                q = q.Where(x => x.ApproveForDeptCode == deptCode || x.ApproveForDeptCode == ApproveForDept.All);
            if (level.HasValue)
                q = q.Where(x => x.Level == level.Value);
            if (requestType.HasValue)
                q = q.Where(x => x.RequestType == requestType.Value);

            var data = await q.OrderBy(x => x.Level)
                .ThenBy(x => x.ApproveForDeptCode).ThenBy(x => x.ApproverName)
                .ToListAsync(ct);

            return ServiceResult<List<ApproverDto>>.Ok(data.Select(ApproverMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] GetList ERROR");
            return ServiceResult<List<ApproverDto>>.Fail("Lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct)
    {
        try
        {
            var depts = await _uow.Repository<F03Department>().Query().AsNoTracking()
                .Where(x => x.IsActive == true).OrderBy(x => x.DeptCode)
                .Select(x => new DepartmentDto { DeptCode = x.DeptCode, DeptName = x.DeptName })
                .ToListAsync(ct);

            depts.Insert(0, new DepartmentDto
            {
                DeptCode = ApproveForDept.All,
                DeptName = "Toàn công ty (ALL)"
            });

            return ServiceResult<List<DepartmentDto>>.Ok(depts);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] GetDepartments ERROR");
            return ServiceResult<List<DepartmentDto>>.Fail("Lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult<List<EmployeeSelectDto>>> GetEmployeesAsync(
        string? deptCode, CancellationToken ct)
    {
        try
        {
            var q = _uow.Repository<F03Employee>().Query().AsNoTracking();
            if (!string.IsNullOrEmpty(deptCode) && deptCode != ApproveForDept.All)
                q = q.Where(x => x.DeptCode == deptCode);

            var deptQuery = _uow.Repository<F03Department>().Query().AsNoTracking();
            var posQuery = _uow.Repository<F03Position>().Query().AsNoTracking();

            var raw = await q.Join(deptQuery,
                    emp => emp.DeptCode, dept => dept.DeptCode,
                    (emp, dept) => new { emp, dept.DeptName })
                .GroupJoin(posQuery,
                    x => x.emp.PositionCode, pos => pos.PositionCode,
                    (x, positions) => new { x.emp, x.DeptName, positions })
                .SelectMany(x => x.positions.DefaultIfEmpty(),
                    (x, pos) => new { x.emp, x.DeptName, Position = pos })
                .OrderBy(x => x.emp.EmployeeName).ToListAsync(ct);

            var employees = raw.Select(x => new EmployeeSelectDto
            {
                EmployeeCode = x.emp.EmployeeCode,
                EmployeeName = x.emp.EmployeeName,
                DeptCode = x.emp.DeptCode,
                DeptName = x.DeptName ?? "",
                CvCode = x.emp.PositionCode,
                Email = x.emp.EmailAddress,
                DefaultLevel = CvCodeRules.ResolveLevel(null, x.emp.PositionCode),
                IsApprover = CvCodeRules.IsApprover(
                    x.emp.PositionCode,
                    x.Position != null && x.Position.IsApprove ? 1 : 0,
                    x.Position != null && x.Position.IsAllowApprove ? 1 : 0)
            }).ToList();

            return ServiceResult<List<EmployeeSelectDto>>.Ok(employees);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] GetEmployees ERROR");
            return ServiceResult<List<EmployeeSelectDto>>.Fail("Lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult> CreateAsync(ApproverDto model, int currentUserId, CancellationToken ct)
    {
        try
        {
            Logger.LogDebugIf(Debug,
                "[APPROVER-MGT] Create: {Name} Lv={Lv} Dept={Dept} Type={Type}",
                model.ApproverName, model.Level, model.ApproveForDeptCode, model.RequestType);

            var validateResult = ValidateModel(model);
            if (!validateResult.IsSuccess) return validateResult;

            var repo = _uow.Repository<F03Approver>();
            bool exists = await repo.Query().AnyAsync(x =>
                x.ApproverCode == model.ApproverCode && x.Level == model.Level &&
                x.RequestType == model.RequestType &&
                x.ApproveForDeptCode == model.ApproveForDeptCode && x.IsActive == true, ct);

            if (exists)
                return ServiceResult.Fail("Approver này đã tồn tại ở cấp duyệt và phòng ban tương ứng.");

            var empInfo = await GetEmployeeWithPositionAsync(model.ApproverCode, ct);
            if (empInfo != null && !CvCodeRules.IsApprover(
                    empInfo.PositionCode, empInfo.IsApprove ? 1 : 0, empInfo.IsAllowApprove ? 1 : 0))
            {
                Logger.LogWarnIf(Debug,
                    "[APPROVER-MGT] PositionCode {Pos} không phải approver — vẫn tạo theo yêu cầu admin",
                    empInfo.PositionCode);
            }

            int resolvedLevel = model.Level > 0 ? model.Level : CvCodeRules.ResolveLevel(null, empInfo?.PositionCode);
            if (resolvedLevel == 0)
                return ServiceResult.Fail("Không xác định được cấp duyệt. Vui lòng chọn thủ công.");

            model.Level = resolvedLevel;
            model.RoleName = RoleNameFromLevel(resolvedLevel, model.RequestType);
            model.ApproveForDeptName = await GetDeptNameAsync(model.ApproveForDeptCode ?? "", ct);
            model.DeptCode = empInfo?.DeptCode ?? "";
            model.DeptName = empInfo?.DeptName ?? "";
            if (string.IsNullOrEmpty(model.ApproverName)) model.ApproverName = empInfo?.EmployeeName ?? "";
            if (string.IsNullOrEmpty(model.ApproverEmail)) model.ApproverEmail = empInfo?.EmailAddress ?? "";

            var entity = ApproverMapper.ToEntity(model, currentUserId);
            await repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug,
                "[APPROVER-MGT] Created Id={Id} {Name} Lv={Lv} Role={Role} Type={Type}",
                entity.Id, entity.ApproverName, entity.Level, entity.RoleName, entity.RequestType);
            return ServiceResult.Ok("Đã thêm approver thành công.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] Create ERROR");
            return ServiceResult.Fail("Lỗi hệ thống khi thêm approver.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(ApproverDto model, int currentUserId, CancellationToken ct)
    {
        try
        {
            Logger.LogDebugIf(Debug, "[APPROVER-MGT] Update Id={Id}", model.Id);
            var repo = _uow.Repository<F03Approver>();
            var entity = await repo.Query().FirstOrDefaultAsync(x => x.Id == model.Id, ct);
            if (entity == null) return ServiceResult.Fail("Không tìm thấy approver.");

            var validateResult = ValidateModel(model);
            if (!validateResult.IsSuccess) return validateResult;

            bool duplicate = await repo.Query().AnyAsync(x =>
                x.Id != model.Id && x.ApproverCode == model.ApproverCode &&
                x.Level == model.Level && x.RequestType == model.RequestType &&
                x.ApproveForDeptCode == model.ApproveForDeptCode && x.IsActive == true, ct);

            if (duplicate)
                return ServiceResult.Fail("Đã tồn tại approver này ở cấp duyệt và phòng ban tương ứng.");

            var empInfo = await GetEmployeeWithPositionAsync(model.ApproverCode, ct);
            int resolvedLevel = model.Level > 0 ? model.Level : CvCodeRules.ResolveLevel(null, empInfo?.PositionCode);
            model.Level = resolvedLevel;
            model.RoleName = RoleNameFromLevel(resolvedLevel, model.RequestType);
            model.ApproveForDeptName = await GetDeptNameAsync(model.ApproveForDeptCode ?? "", ct);
            model.DeptCode = empInfo?.DeptCode ?? entity.ApproverDeptCode;
            model.DeptName = empInfo?.DeptName ?? entity.ApproverDeptName;
            if (string.IsNullOrEmpty(model.ApproverName)) model.ApproverName = empInfo?.EmployeeName ?? entity.ApproverName;
            if (string.IsNullOrEmpty(model.ApproverEmail)) model.ApproverEmail = empInfo?.EmailAddress ?? entity.ApproverEmail;

            ApproverMapper.ApplyUpdate(entity, model, currentUserId);
            if (empInfo != null)
            {
                entity.ApproverDeptCode = empInfo.DeptCode ?? entity.ApproverDeptCode;
                entity.ApproverDeptName = empInfo.DeptName ?? entity.ApproverDeptName;
            }

            repo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            Logger.LogInfoIf(Debug, "[APPROVER-MGT] Updated Id={Id}", entity.Id);
            return ServiceResult.Ok("Đã cập nhật approver.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] Update ERROR Id={Id}", model.Id);
            return ServiceResult.Fail("Lỗi hệ thống khi cập nhật approver.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, int currentUserId, CancellationToken ct)
    {
        try
        {
            var repo = _uow.Repository<F03Approver>();
            var entity = await repo.Query().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return ServiceResult.Fail("Không tìm thấy.");
            entity.IsActive = false;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
            repo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            Logger.LogInfoIf(Debug, "[APPROVER-MGT] Soft-deleted Id={Id} {Name}", id, entity.ApproverName);
            return ServiceResult.Ok("Đã loại approver này khỏi danh sách duyệt.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] Delete ERROR Id={Id}", id);
            return ServiceResult.Fail("Lỗi hệ thống khi xóa approver.");
        }
    }

    public async Task<ServiceResult> ToggleActiveAsync(int id, int currentUserId, CancellationToken ct)
    {
        try
        {
            var repo = _uow.Repository<F03Approver>();
            var entity = await repo.Query().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return ServiceResult.Fail("Không tìm thấy.");
            entity.IsActive = entity.IsActive != true;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
            repo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            var statusText = entity.IsActive == true ? "kích hoạt" : "vô hiệu hóa";
            Logger.LogInfoIf(Debug, "[APPROVER-MGT] Toggled Id={Id} → IsActive={Active}", id, entity.IsActive);
            return ServiceResult.Ok($"Đã {statusText} approver.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[APPROVER-MGT] Toggle ERROR Id={Id}", id);
            return ServiceResult.Fail("Lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult<List<ApproverSyncProposalDto>>> GetSyncProposalsAsync(CancellationToken ct)
    {
        try
        {
            var rows=await _uow.Repository<F03SyncReviewFlag>().Query().AsNoTracking()
                .Where(x=>!x.IsResolved&&x.FlagType=="ApproverConfigurationChanged")
                .OrderByDescending(x=>x.DetectedAt)
                .Select(x=>new ApproverSyncProposalDto {
                    Id=x.Id, EmployeeCode=x.EntityKey, Message=x.Message,
                    ChangeType=x.OldDeptCode!=x.NewDeptCode&&x.OldPositionCode!=x.NewPositionCode?"Phòng ban + Chức vụ":x.OldDeptCode!=x.NewDeptCode?"Phòng ban":"Chức vụ",
                    OldDeptCode=x.OldDeptCode??"",OldPositionCode=x.OldPositionCode??"",NewDeptCode=x.NewDeptCode??"",NewPositionCode=x.NewPositionCode??"",
                    CurrentApproverId=x.CurrentApproverId,CurrentApproverCode=x.CurrentApproverCode??"",CurrentLevel=x.CurrentLevel,CurrentRoleName=x.CurrentRoleName??"",CurrentApproveForDeptCode=x.CurrentApproveForDeptCode??"",
                    SuggestedApproverCode=x.SuggestedApproverCode,SuggestedLevel=x.SuggestedLevel,SuggestedRoleName=x.SuggestedRoleName,SuggestedApproveForDeptCode=x.SuggestedApproveForDeptCode,DetectedAt=x.DetectedAt
                }).ToListAsync(ct);
            return ServiceResult<List<ApproverSyncProposalDto>>.Ok(rows);
        } catch(Exception ex) { Logger.LogError(ex,"[APPROVER-MGT] GetSyncProposals ERROR"); return ServiceResult<List<ApproverSyncProposalDto>>.Fail("Lỗi tải đề xuất đồng bộ Approver."); }
    }

    public async Task<ServiceResult> AcceptSyncProposalAsync(int flagId,int currentUserId,CancellationToken ct)
    {
        var flag=await _uow.Repository<F03SyncReviewFlag>().Query().FirstOrDefaultAsync(x=>x.Id==flagId&&!x.IsResolved&&x.FlagType=="ApproverConfigurationChanged",ct);
        if(flag==null) return ServiceResult.Fail("Không tìm thấy đề xuất hoặc đề xuất đã được xử lý.");
        if(!flag.CurrentApproverId.HasValue||!flag.SuggestedLevel.HasValue||string.IsNullOrWhiteSpace(flag.SuggestedApproverCode)) return ServiceResult.Fail("HRM chưa xác định được người duyệt mới duy nhất. Hãy chỉnh thủ công.");
        var a=await _uow.Repository<F03Approver>().Query().FirstOrDefaultAsync(x=>x.Id==flag.CurrentApproverId.Value,ct);
        if(a==null) return ServiceResult.Fail("Cấu hình Approver hiện tại không còn tồn tại.");
        var e=await GetEmployeeWithPositionAsync(flag.SuggestedApproverCode,ct);
        if(e==null) return ServiceResult.Fail("Người duyệt đề xuất không còn hoạt động.");
        a.ApproverCode=flag.SuggestedApproverCode;a.ApproverName=e.EmployeeName;a.ApproverEmail=e.EmailAddress;a.PositionCode=e.PositionCode;a.ApproverDeptCode=e.DeptCode;a.ApproverDeptName=e.DeptName;a.Level=flag.SuggestedLevel.Value;a.RoleName=flag.SuggestedRoleName??RoleNameFromLevel(a.Level,a.RequestType);
        if(a.ApproveForDeptCode!=ApproveForDept.All&&!string.IsNullOrWhiteSpace(flag.SuggestedApproveForDeptCode)){a.ApproveForDeptCode=flag.SuggestedApproveForDeptCode;a.ApproveForDeptName=await GetDeptNameAsync(a.ApproveForDeptCode,ct);}
        a.ModifiedBy=currentUserId;a.ModifiedAt=DateTime.Now;flag.IsResolved=true;flag.ResolvedAt=DateTime.Now;flag.ResolvedBy=currentUserId.ToString();flag.Decision="AcceptedHrmProposal";
        await _uow.SaveChangesAsync(ct);return ServiceResult.Ok("Đã áp dụng đề xuất HRM vào cấu hình Approver.");
    }

    public async Task<ServiceResult> KeepSyncProposalAsync(int flagId,int currentUserId,CancellationToken ct)
    {
        var flag=await _uow.Repository<F03SyncReviewFlag>().Query().FirstOrDefaultAsync(x=>x.Id==flagId&&!x.IsResolved&&x.FlagType=="ApproverConfigurationChanged",ct);
        if(flag==null) return ServiceResult.Fail("Không tìm thấy đề xuất hoặc đề xuất đã được xử lý.");
        flag.IsResolved=true;flag.ResolvedAt=DateTime.Now;flag.ResolvedBy=currentUserId.ToString();flag.Decision="KeepCurrentConfiguration";await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã giữ nguyên cấu hình Approver hiện tại.");
    }

    private static ServiceResult ValidateModel(ApproverDto model)
    {
        if (string.IsNullOrWhiteSpace(model.ApproverCode)) return ServiceResult.Fail("Chưa chọn nhân viên.");
        if (model.RequestType is not RequestModule.Leave
            and not RequestModule.Overtime
            and not RequestModule.Trip
            and not RequestModule.Equipment)
            return ServiceResult.Fail("Loại yêu cầu không hợp lệ.");

        if (model.Level < 0 || model.Level > 7)
            return ServiceResult.Fail("Cấp duyệt không hợp lệ.");

        if (model.Level > 0)
        {
            var validLevel = model.RequestType switch
            {
                RequestModule.Leave => model.Level is 1 or 2 or 3,
                RequestModule.Overtime => model.Level is 3 or 5 or 6 or 7,
                RequestModule.Trip => model.Level is 1 or 2 or 3,
                RequestModule.Equipment => model.Level is 1 or 2 or 3,
                _ => false
            };

            if (!validLevel)
                return ServiceResult.Fail($"Cấp duyệt {model.Level} không hợp lệ cho {model.RequestType}.");
        }

        if (string.IsNullOrWhiteSpace(model.ApproveForDeptCode))
            return ServiceResult.Fail("Chưa chọn phòng ban được duyệt.");

        if (string.IsNullOrWhiteSpace(model.ApproverEmail) || !model.ApproverEmail.Contains('@'))
            return ServiceResult.Fail("Email approver không hợp lệ.");

        return ServiceResult.Ok();
    }

    private static string RoleNameFromLevel(int level, RequestModule requestType) =>
        requestType switch
        {
        RequestModule.Overtime when level == 3 => ApproverRole.SubLeader,
        RequestModule.Overtime when level == 5 => ApproverRole.Chief,
        RequestModule.Overtime when level == 6 => ApproverRole.Manager,
        RequestModule.Overtime when level == 7 => ApproverRole.GM,
        _ when level == 1 => ApproverRole.SubLeader,
        _ when level == 2 => ApproverRole.Manager,
        _ when level == 3 => ApproverRole.GM,
        _ when level == 4 => ApproverRole.GM,
        _ => $"Level{level}"
    };

    private static string LevelDisplayName(int level) => level switch
    {
        ApprovalLevel.SubLeader => "Sub-Leader / Leader",
        ApprovalLevel.Chief => "Ast.Chief / Chief",
        ApprovalLevel.Manager => "Manager / Sen.Manager",
        ApprovalLevel.GM => "Giám đốc (GM)",
        ApprovalLevel.Union => "BCH Công đoàn",
        _ => $"Cấp {level}"
    };

    private async Task<string> GetDeptNameAsync(string deptCode, CancellationToken ct)
    {
        if (deptCode == ApproveForDept.All) return "Toàn công ty";
        return await _uow.Repository<F03Department>().Query()
            .Where(x => x.DeptCode == deptCode).Select(x => x.DeptName)
            .FirstOrDefaultAsync(ct) ?? deptCode;
    }

    private async Task<EmployeeWithPositionInfo?> GetEmployeeWithPositionAsync(
        string employeeCode, CancellationToken ct)
    {
        var empQuery = _uow.Repository<F03Employee>().Query().AsNoTracking();
        var deptQuery = _uow.Repository<F03Department>().Query().AsNoTracking();
        var posQuery = _uow.Repository<F03Position>().Query().AsNoTracking();

        var result = await empQuery.Where(e => e.EmployeeCode == employeeCode)
            .Join(deptQuery, e => e.DeptCode, d => d.DeptCode,
                (e, d) => new { e, d.DeptName })
            .GroupJoin(posQuery, x => x.e.PositionCode, p => p.PositionCode,
                (x, positions) => new { x.e, x.DeptName, positions })
            .SelectMany(x => x.positions.DefaultIfEmpty(),
                (x, p) => new { x.e, x.DeptName, Position = p })
            .FirstOrDefaultAsync(ct);

        if (result == null) return null;
        return new EmployeeWithPositionInfo
        {
            EmployeeName = result.e.EmployeeName,
            DeptCode = result.e.DeptCode,
            DeptName = result.DeptName,
            PositionCode = result.e.PositionCode,
            EmailAddress = result.e.EmailAddress,
            IsApprove = result.Position?.IsApprove ?? false,
            IsAllowApprove = result.Position?.IsAllowApprove ?? false
        };
    }

    private class EmployeeWithPositionInfo
    {
        public string EmployeeName { get; set; } = "";
        public string DeptCode { get; set; } = "";
        public string DeptName { get; set; } = "";
        public string PositionCode { get; set; } = "";
        public string EmailAddress { get; set; } = "";
        public bool IsApprove { get; set; }
        public bool IsAllowApprove { get; set; }
    }
}
