using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Rules;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Extensions;
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
                            RoleName = RoleNameFromLevel(levelGroup.Key),
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
            model.RoleName = RoleNameFromLevel(resolvedLevel);
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
            model.RoleName = RoleNameFromLevel(resolvedLevel);
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

    public async Task<int> GetRequesterLevelAsync(string employeeCode, RequestModule requestType, CancellationToken ct)
    {
        var levelFromTable = await _uow.Repository<F03Approver>().Query()
            .Where(x => x.ApproverCode == employeeCode && x.RequestType == requestType && x.IsActive == true)
            .Select(x => x.Level).FirstOrDefaultAsync(ct);
        if (levelFromTable > 0) return levelFromTable;

        var emp = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode)
            .Select(x => new { x.LevelApprove, x.PositionCode })
            .FirstOrDefaultAsync(ct);

        return emp == null ? 0 : CvCodeRules.ResolveLevel(emp.LevelApprove, emp.PositionCode);
    }

    public async Task<F03Approver?> GetApproverAsync(
        int level, string approveForDeptCode, RequestModule requestType, CancellationToken ct)
    {
        var q = _uow.Repository<F03Approver>().Query().AsNoTracking()
            .Where(x => x.Level == level && x.RequestType == requestType && x.IsActive == true);
        var byDept = await q.Where(x => x.ApproveForDeptCode == approveForDeptCode).FirstOrDefaultAsync(ct);
        if (byDept != null) return byDept;
        return await q.Where(x => x.ApproveForDeptCode == ApproveForDept.All).FirstOrDefaultAsync(ct);
    }

    public async Task<List<F03Approver>> GetApproversForDeptAsync(
        string approveForDeptCode, RequestModule requestType, CancellationToken ct)
    {
        var all = await _uow.Repository<F03Approver>().Query().AsNoTracking()
            .Where(x => x.RequestType == requestType && x.IsActive == true &&
                        (x.ApproveForDeptCode == approveForDeptCode || x.ApproveForDeptCode == ApproveForDept.All))
            .OrderBy(x => x.Level).ThenBy(x => x.ApproverName).ToListAsync(ct);

        return all.GroupBy(x => x.Level).SelectMany(g =>
        {
            var byDept = g.Where(x => x.ApproveForDeptCode == approveForDeptCode).ToList();
            return byDept.Any() ? byDept : g.Where(x => x.ApproveForDeptCode == ApproveForDept.All).ToList();
        }).OrderBy(x => x.Level).ToList();
    }

    public async Task<string> ResolveApproverEmailAsync(
        string employeeCode, RequestModule requestType, CancellationToken ct = default)
    {
        return await _uow.Repository<F03Approver>().Query().AsNoTracking()
            .Where(x => x.ApproverCode == employeeCode && x.RequestType == requestType && x.IsActive == true)
            .Select(x => x.ApproverEmail).FirstOrDefaultAsync(ct) ?? "";
    }

    private static ServiceResult ValidateModel(ApproverDto model)
    {
        if (string.IsNullOrWhiteSpace(model.ApproverCode)) return ServiceResult.Fail("Chưa chọn nhân viên.");
        if (model.RequestType != RequestModule.Leave && model.RequestType != RequestModule.Overtime)
            return ServiceResult.Fail("Loại yêu cầu không hợp lệ (LEAVE hoặc OT).");
        if (model.Level < 0 || model.Level > ApprovalLevel.Union)
            return ServiceResult.Fail($"Cấp duyệt không hợp lệ (1–{ApprovalLevel.Union}).");
        if (string.IsNullOrWhiteSpace(model.ApproveForDeptCode)) return ServiceResult.Fail("Chưa chọn phòng ban được duyệt.");
        if (string.IsNullOrWhiteSpace(model.ApproverEmail) || !model.ApproverEmail.Contains('@'))
            return ServiceResult.Fail("Email approver không hợp lệ.");
        if (model.Level == ApprovalLevel.Union && model.RequestType != RequestModule.Overtime)
            return ServiceResult.Fail("Cấp Công đoàn (Level 5) chỉ áp dụng cho OT.");
        return ServiceResult.Ok();
    }

    private static string RoleNameFromLevel(int level) => level switch
    {
        ApprovalLevel.SubLeader => ApproverRole.SubLeader,
        ApprovalLevel.Chief => ApproverRole.Chief,
        ApprovalLevel.Manager => ApproverRole.Manager,
        ApprovalLevel.GM => ApproverRole.GM,
        ApprovalLevel.Union => ApproverRole.Union,
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
