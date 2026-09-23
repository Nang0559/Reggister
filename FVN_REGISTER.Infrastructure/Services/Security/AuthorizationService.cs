using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Security;

public sealed class AuthorizationService : BaseService<AuthorizationService>, IAuthorizationService
{
    private readonly IUnitOfWork _uow;
    private readonly FVN_REGISTER.Application.Interfaces.Auths.ISessionService _sessionService;
    private readonly FVN_REGISTER.Application.Interfaces.Auths.IAuditService _auditService;

    public AuthorizationService(
        IUnitOfWork uow,
        ILogger<AuthorizationService> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        FVN_REGISTER.Application.Interfaces.Auths.ISessionService sessionService,
        FVN_REGISTER.Application.Interfaces.Auths.IAuditService auditService)
        : base(logger, options)
    {
        _uow = uow;
        _sessionService = sessionService;
        _auditService = auditService;
    }

    public async Task<bool> HasAsync(
        UserIdentityDto user,
        int functionCode,
        CancellationToken ct = default)
    {
        if (user.UserId <= 0)
            return false;

        var snapshot = await GetSnapshotAsync(user.UserId, ct);
        return snapshot.Has(functionCode);
    }

    public async Task<string> GetScopeAsync(int userId, int functionCode, CancellationToken ct = default)
    {
        if (userId <= 0)
            return AuthorizationScopeCodes.None;

        var scopes = await (
            from ur in _uow.Repository<F03UserRole>().Query().AsNoTracking()
            join rf in _uow.Repository<F03RoleFunction>().Query().AsNoTracking()
                on ur.IdRole equals rf.IdRole
            join r in _uow.Repository<F03Role>().Query().AsNoTracking()
                on ur.IdRole equals r.Id
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on rf.IdFunction equals f.Id
            where ur.IdUser == userId
                && r.IsActive == true
                && (f.IsActive ?? true)
                && f.FunctionCode == functionCode
            select f.ScopeCode
        ).ToListAsync(ct);

        // Legacy direct grants have no reliable scope metadata in old databases.
        // During migration, treat an unscoped legacy grant as Own rather than
        // silently expanding it to Department/All.
        scopes.AddRange(await (
            from uf in _uow.Repository<F03UserFunction>().Query().AsNoTracking()
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on uf.IdFunction equals f.Id
            where uf.IdUser == userId
                && (f.IsActive ?? true)
                && f.FunctionCode == functionCode
            select f.ScopeCode
        ).ToListAsync(ct));

        return AuthorizationScopePolicy.ResolveEffectiveScope(scopes);
    }

    public async Task<bool> CanAccessAsync(
        UserIdentityDto user,
        int functionCode,
        string? employeeCode,
        string? deptCode,
        CancellationToken ct = default)
    {
        if (user.UserId <= 0)
            return false;

        var scope = await GetScopeAsync(user.UserId, functionCode, ct);
        if (AuthorizationScopePolicy.CanAccess(
            scope,
            user.EmployeeCode,
            user.DeptCode,
            employeeCode,
            deptCode))
            return true;

        // ManagedScope is an additional data-scope grant. It never grants the
        // capability itself and therefore cannot bypass HasAsync/RoleFunction.
        if (string.Equals(scope, AuthorizationScopeCodes.None, StringComparison.OrdinalIgnoreCase))
            return false;

        var managed = await GetManagedScopesAsync(user.UserId, ct);
        if (managed.Count == 0)
            return false;

        return await IsWithinManagedScopeAsync(managed, employeeCode, deptCode, ct);
    }

    public async Task<List<ManagedScopeDto>> GetManagedScopesAsync(
        int userId,
        CancellationToken ct = default)
    {
        if (userId <= 0)
            return new();

        var employeeCode = await _uow.Repository<F03User>().Query()
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.EmployeeCode)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(employeeCode))
            return new();

        return await _uow.Repository<F03ManagedScope>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == employeeCode)
            .OrderBy(x => x.NodeType)
            .ThenBy(x => x.NodeCode)
            .Select(x => new ManagedScopeDto
            {
                Id = x.Id,
                EmployeeCode = x.EmployeeCode,
                NodeType = x.NodeType,
                NodeCode = x.NodeCode,
                FactoryCode = x.FactoryCode,
                DeptCode = x.DeptCode,
                SubDepartmentCode = x.SubDepartmentCode,
                IncludeChildren = x.IncludeChildren,
                Remark = x.Remark
            })
            .ToListAsync(ct);
    }

    public async Task<PermissionSnapshotDto> ReplaceManagedScopesAsync(
        int userId,
        IReadOnlyCollection<ManagedScopeRequest> scopes,
        int actorUserId,
        CancellationToken ct = default)
    {
        if (userId <= 0 || actorUserId <= 0)
            throw new InvalidOperationException("UserId/ActorUserId không hợp lệ.");

        var employeeCode = await _uow.Repository<F03User>().Query()
            .Where(x => x.Id == userId)
            .Select(x => x.EmployeeCode)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new InvalidOperationException("Tài khoản chưa liên kết nhân viên.");

        foreach (var scope in scopes)
        {
            var nodeType = (scope.NodeType ?? string.Empty).Trim();
            if (nodeType is not ("Company" or "Factory" or "Department" or "SubDepartment"))
                throw new InvalidOperationException($"NodeType ManagedScope không hợp lệ: {nodeType}.");

            if (nodeType != "Company" &&
                string.IsNullOrWhiteSpace(scope.NodeCode) &&
                string.IsNullOrWhiteSpace(scope.DeptCode) &&
                string.IsNullOrWhiteSpace(scope.SubDepartmentCode) &&
                string.IsNullOrWhiteSpace(scope.FactoryCode))
                throw new InvalidOperationException($"ManagedScope {nodeType} phải có NodeCode hoặc mã node tổ chức.");
        }

        var repo = _uow.Repository<F03ManagedScope>();
        var existing = await repo.Query()
            .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
            .ToListAsync(ct);

        foreach (var row in existing)
        {
            row.IsActive = false;
            row.ModifiedBy = actorUserId;
            row.ModifiedAt = DateTime.Now;
            row.LastModifiedSource = "SECURITY_MANAGED_SCOPE_REPLACE";
        }

        foreach (var scope in scopes)
        {
            await repo.AddAsync(new F03ManagedScope
            {
                EmployeeCode = employeeCode,
                NodeType = scope.NodeType.Trim(),
                NodeCode = scope.NodeCode?.Trim(),
                FactoryCode = scope.FactoryCode?.Trim(),
                DeptCode = scope.DeptCode?.Trim(),
                SubDepartmentCode = scope.SubDepartmentCode?.Trim(),
                IncludeChildren = scope.IncludeChildren,
                Remark = scope.Remark?.Trim(),
                CreatedBy = actorUserId,
                LastModifiedSource = "SECURITY_MANAGED_SCOPE"
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        await _auditService.LogAction(
            "SECURITY_MANAGED_SCOPE_CHANGED",
            actorUserId,
            $"UserId={userId}; EmployeeCode={employeeCode}; Count={scopes.Count}",
            ct: ct);

        return await GetSnapshotAsync(userId, ct);
    }

    public async Task<EffectivePermissionPreviewDto> GetEffectivePermissionPreviewAsync(
        int userId,
        CancellationToken ct = default)
    {
        var snapshot = await GetSnapshotAsync(userId, ct);
        var user = await _uow.Repository<F03User>().Query()
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new { x.EmployeeCode, x.FullName, x.DeptCode, x.Cvcode })
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Không tìm thấy tài khoản.");

        var roles = await GetRolesAsync(ct);
        var roleNames = roles
            .Where(x => snapshot.RoleCodes.Contains(x.RoleCode))
            .Select(x => x.RoleName)
            .ToList();

        var managedScopes = await GetManagedScopesAsync(userId, ct);
        var policies = await _uow.Repository<F03ApprovalPolicy>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true
                && x.ApprovalPositionCode == user.Cvcode)
            .OrderBy(x => x.RequestType)
            .ThenBy(x => x.Level)
            .ThenBy(x => x.Sequence)
            .Select(x => new EffectiveApprovalPolicyDto
            {
                RequestType = (int)x.RequestType,
                RequestTypeName = x.RequestType.ToString(),
                DeptCode = x.DeptCode,
                PositionCode = x.PositionCode,
                ApprovalPositionCode = x.ApprovalPositionCode,
                Level = x.Level,
                Sequence = x.Sequence,
                LevelName = x.LevelName,
                RoleName = x.RoleName
            })
            .ToListAsync(ct);

        var actions = snapshot.Functions
            .Where(x => !string.IsNullOrWhiteSpace(x.ModuleCode))
            .Select(x => new EffectivePermissionActionDto
            {
                FunctionCode = x.FunctionCode,
                ModuleCode = x.ModuleCode!,
                ActionCode = x.ActionCode ?? string.Empty,
                ScopeCode = x.ScopeCode
            })
            .ToList();

        var visibleMenus = actions
            .Select(x => x.ModuleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var constraints = new List<string>
        {
            "Business State luôn được kiểm tra server-side; capability không tự bỏ qua state machine."
        };
        if (actions.Any(x => x.ModuleCode.Equals("Leave", StringComparison.OrdinalIgnoreCase)))
            constraints.Add("Leave: Draft/Pending/Approved/Rejected/Cancelled giới hạn Create/Edit/Cancel/Approve theo state.");
        if (actions.Any(x => x.ModuleCode.Equals("OT", StringComparison.OrdinalIgnoreCase)))
            constraints.Add("OT: Draft/Pending/Approved/Rejected/Cancelled giới hạn Create/Edit/Cancel/Approve/Reconcile theo state.");
        if (actions.Any(x => x.ModuleCode.Equals("Trip", StringComparison.OrdinalIgnoreCase)))
            constraints.Add("Trip: Draft/Pending/Approved/Rejected/Cancelled giới hạn Create/Edit/Cancel/Approve theo state.");
        if (actions.Any(x => x.ModuleCode.Equals("Equipment", StringComparison.OrdinalIgnoreCase)))
            constraints.Add("Equipment: Assign/Transfer/Return/Liquidate/Repair/Approve còn bị giới hạn bởi asset/request state và field rule.");
        if (actions.Any(x => x.ModuleCode.Equals("Payroll", StringComparison.OrdinalIgnoreCase)))
            constraints.Add("Payroll: không Lock/Export khi reconciliation/correction còn unresolved hoặc snapshot stale.");

        return new EffectivePermissionPreviewDto
        {
            UserId = userId,
            EmployeeCode = user.EmployeeCode,
            EmployeeName = user.FullName,
            DeptCode = user.DeptCode,
            PositionCode = user.Cvcode,
            RoleCodes = snapshot.RoleCodes,
            RoleNames = roleNames,
            VisibleMenus = visibleMenus,
            Actions = actions,
            ManagedScopes = managedScopes,
            ApprovalPolicies = policies,
            BusinessStateConstraints = constraints
        };
    }

    private async Task<bool> IsWithinManagedScopeAsync(
        IReadOnlyCollection<ManagedScopeDto> scopes,
        string? employeeCode,
        string? deptCode,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(employeeCode) && string.IsNullOrWhiteSpace(deptCode))
            return false;

        var target = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true &&
                        ((employeeCode != null && x.EmployeeCode == employeeCode) ||
                         (employeeCode == null && deptCode != null && x.DeptCode == deptCode)))
            .Select(x => new { x.EmployeeCode, x.DeptCode })
            .FirstOrDefaultAsync(ct);

        var targetDept = target?.DeptCode ?? deptCode;
        if (string.IsNullOrWhiteSpace(targetDept))
            return false;

        var departments = await _uow.Repository<F03Department>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .Select(x => new { x.DeptCode, x.ParentDeptCode })
            .ToListAsync(ct);

        var parents = departments.ToDictionary(x => x.DeptCode, x => x.ParentDeptCode);
        foreach (var scope in scopes)
        {
            if (scope.NodeType.Equals("Company", StringComparison.OrdinalIgnoreCase))
                return true;

            var node = scope.SubDepartmentCode ?? scope.DeptCode ?? scope.NodeCode;
            if (scope.NodeType.Equals("Factory", StringComparison.OrdinalIgnoreCase))
            {
                // Current HR master does not persist FactoryCode on F03Employees yet.
                // A Factory scope can therefore use DeptCode/NodeCode as its HRM anchor.
                node ??= scope.FactoryCode;
            }

            if (string.IsNullOrWhiteSpace(node))
                continue;

            if (string.Equals(targetDept, node, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!scope.IncludeChildren)
                continue;

            var cursor = targetDept;
            while (parents.TryGetValue(cursor, out var parent) && !string.IsNullOrWhiteSpace(parent))
            {
                if (string.Equals(parent, node, StringComparison.OrdinalIgnoreCase))
                    return true;
                cursor = parent!;
            }
        }

        return false;
    }

    public async Task<PermissionSnapshotDto> GetSnapshotAsync(
        int userId,
        CancellationToken ct = default)
    {
        var roleCodes = await (
            from ur in _uow.Repository<F03UserRole>().Query().AsNoTracking()
            join r in _uow.Repository<F03Role>().Query().AsNoTracking()
                on ur.IdRole equals r.Id
            where ur.IdUser == userId && r.IsActive == true
            select r.RoleCode
        ).Distinct().ToListAsync(ct);

        // Compatibility: primary PermissionCode is still an effective role while
        // deployments are migrating from the legacy single-role model.
        var legacyRole = await _uow.Repository<F03User>().Query()
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.PermissionCode)
            .FirstOrDefaultAsync(ct);

        if (legacyRole.HasValue && !roleCodes.Contains(legacyRole.Value))
            roleCodes.Add(legacyRole.Value);

        var functionCodes = await (
            from ur in _uow.Repository<F03UserRole>().Query().AsNoTracking()
            join rf in _uow.Repository<F03RoleFunction>().Query().AsNoTracking()
                on ur.IdRole equals rf.IdRole
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on rf.IdFunction equals f.Id
            join r in _uow.Repository<F03Role>().Query().AsNoTracking()
                on ur.IdRole equals r.Id
            where ur.IdUser == userId && r.IsActive == true && (f.IsActive ?? true)
            select f
        ).Distinct().ToListAsync(ct);

        // Legacy direct grants remain effective during migration.
        var legacyFunctions = await (
            from uf in _uow.Repository<F03UserFunction>().Query().AsNoTracking()
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on uf.IdFunction equals f.Id
            where uf.IdUser == userId && (f.IsActive ?? true)
            select f
        ).ToListAsync(ct);

        var functions = functionCodes
            .Concat(legacyFunctions)
            .GroupBy(x => x.FunctionCode)
            .Select(g => g.OrderBy(x => x.DisplayOrder).First())
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.FunctionCode)
            .Select(x => new SecurityFunctionDto
            {
                IdFunction = x.Id,
                FunctionCode = x.FunctionCode,
                FunctionName = x.FunctionName,
                Detail = x.Detail,
                ModuleCode = x.ModuleCode,
                ActionCode = x.ActionCode,
                ScopeCode = x.ScopeCode,
                DisplayOrder = x.DisplayOrder
            })
            .ToList();

        return new PermissionSnapshotDto
        {
            UserId = userId,
            RoleCodes = roleCodes,
            Functions = functions,
            FunctionCodes = functions.Select(x => x.FunctionCode).ToHashSet()
        };
    }

    public async Task<List<SecurityRoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await _uow.Repository<F03Role>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.RoleCode)
            .ToListAsync(ct);

        var roleIds = roles.Select(x => x.Id).ToList();
        var map = await (
            from rf in _uow.Repository<F03RoleFunction>().Query().AsNoTracking()
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on rf.IdFunction equals f.Id
            where roleIds.Contains(rf.IdRole)
            select new { rf.IdRole, f.FunctionCode }
        ).ToListAsync(ct);

        return roles.Select(r => new SecurityRoleDto
        {
            IdRole = r.Id,
            RoleCode = r.RoleCode,
            RoleName = r.RoleName,
            Detail = r.Detail,
            IsSystem = r.IsSystem,
            IsActive = r.IsActive == true,
            FunctionCodes = map.Where(x => x.IdRole == r.Id)
                .Select(x => x.FunctionCode).Distinct().OrderBy(x => x).ToList()
        }).ToList();
    }

    public async Task<List<SecurityFunctionDto>> GetFunctionsAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<F03Function>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.FunctionCode)
            .Select(x => new SecurityFunctionDto
            {
                IdFunction = x.Id,
                FunctionCode = x.FunctionCode,
                FunctionName = x.FunctionName,
                Detail = x.Detail,
                ModuleCode = x.ModuleCode,
                ActionCode = x.ActionCode,
                ScopeCode = x.ScopeCode,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync(ct);
    }

    public async Task<PermissionSnapshotDto> SetUserRolesAsync(
        int userId,
        IReadOnlyCollection<int> roleCodes,
        int actorUserId,
        CancellationToken ct = default)
    {
        if (userId == actorUserId)
            throw new InvalidOperationException("Không thể tự thay đổi role của chính mình.");
        if (roleCodes == null || roleCodes.Count == 0)
            throw new InvalidOperationException("Tài khoản phải có ít nhất một role.");

        var roles = await _uow.Repository<F03Role>().Query()
            .Where(x => x.IsActive == true && roleCodes.Contains(x.RoleCode))
            .ToListAsync(ct);

        if (roles.Count != roleCodes.Distinct().Count())
            throw new InvalidOperationException("Một hoặc nhiều role không tồn tại.");

        var repo = _uow.Repository<F03UserRole>();
        var existing = await repo.Query().Where(x => x.IdUser == userId).ToListAsync(ct);
        foreach (var row in existing)
            repo.Remove(row);

        foreach (var role in roles)
        {
            await repo.AddAsync(new F03UserRole
            {
                IdUser = userId,
                IdRole = role.Id,
                IsPrimary = role.RoleCode == roleCodes.FirstOrDefault(),
                CreatedBy = actorUserId,
                CreatedAt = DateTime.Now,
                ModifiedBy = actorUserId,
                ModifiedAt = DateTime.Now
            }, ct);
        }

        // Keep legacy primary PermissionCode synchronized for existing login/UI code.
        var primaryRoleCode = roleCodes.FirstOrDefault();
        var user = await _uow.Repository<F03User>().Query()
            .FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user != null && primaryRoleCode > 0)
        {
            user.PermissionCode = primaryRoleCode;
            user.ModifiedBy = actorUserId;
            user.ModifiedAt = DateTime.Now;
        }

        await _uow.SaveChangesAsync(ct);
        await _auditService.LogAction(
            "SECURITY_USER_ROLES_CHANGED",
            actorUserId,
            $"UserId={userId}; Roles={string.Join(',', roleCodes.Distinct().OrderBy(x => x))}",
            ct: ct);
        await _sessionService.RevokeAllAsync(userId, ct);
        return await GetSnapshotAsync(userId, ct);
    }
    public async Task<SecurityRoleDto> SetRoleFunctionsAsync(
        int roleCode,
        IReadOnlyCollection<int> functionCodes,
        int actorUserId,
        CancellationToken ct = default)
    {
        if (functionCodes == null)
            throw new InvalidOperationException("FunctionCodes không hợp lệ.");

        var role = await _uow.Repository<F03Role>().Query()
            .FirstOrDefaultAsync(x => x.RoleCode == roleCode && x.IsActive == true, ct);

        if (role == null)
            throw new InvalidOperationException("Role không tồn tại hoặc đã ngừng hoạt động.");

        var codes = functionCodes.Distinct().ToList();
        var functions = await _uow.Repository<F03Function>().Query()
            .Where(x => codes.Contains(x.FunctionCode) && (x.IsActive ?? true))
            .ToListAsync(ct);

        if (functions.Count != codes.Count)
            throw new InvalidOperationException("Một hoặc nhiều function không tồn tại hoặc đã ngừng hoạt động.");

        var repo = _uow.Repository<F03RoleFunction>();
        var existing = await repo.Query().Where(x => x.IdRole == role.Id).ToListAsync(ct);
        foreach (var row in existing)
            repo.Remove(row);

        foreach (var function in functions)
        {
            await repo.AddAsync(new F03RoleFunction
            {
                IdRole = role.Id,
                IdFunction = function.Id,
                CreatedBy = actorUserId,
                CreatedAt = DateTime.Now
            }, ct);
        }

        role.ModifiedBy = actorUserId;
        role.ModifiedAt = DateTime.Now;

        await _uow.SaveChangesAsync(ct);
        await _auditService.LogAction(
            "SECURITY_ROLE_FUNCTIONS_CHANGED",
            actorUserId,
            $"RoleCode={roleCode}; Functions={string.Join(',', codes.OrderBy(x => x))}",
            ct: ct);
        return (await GetRolesAsync(ct)).Single(x => x.RoleCode == roleCode);
    }

}
