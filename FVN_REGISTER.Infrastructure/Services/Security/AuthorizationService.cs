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
                && r.IsActive
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

        var normalizedScopes = scopes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedScopes.Any(x => string.Equals(x, AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.All;
        if (normalizedScopes.Any(x => string.Equals(x, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.Department;
        if (normalizedScopes.Any(x => string.Equals(x, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.Employee;
        if (normalizedScopes.Any(x => string.Equals(x, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.Own;

        return AuthorizationScopeCodes.None;
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

        return scope switch
        {
            AuthorizationScopeCodes.All => true,
            AuthorizationScopeCodes.Department =>
                !string.IsNullOrWhiteSpace(user.DeptCode)
                && !string.IsNullOrWhiteSpace(deptCode)
                && string.Equals(user.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase),
            AuthorizationScopeCodes.Own =>
                !string.IsNullOrWhiteSpace(user.EmployeeCode)
                && !string.IsNullOrWhiteSpace(employeeCode)
                && string.Equals(user.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase),
            AuthorizationScopeCodes.Employee =>
                !string.IsNullOrWhiteSpace(employeeCode)
                && string.Equals(user.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public async Task<PermissionSnapshotDto> GetSnapshotAsync(
        int userId,
        CancellationToken ct = default)
    {
        var roleCodes = await (
            from ur in _uow.Repository<F03UserRole>().Query().AsNoTracking()
            join r in _uow.Repository<F03Role>().Query().AsNoTracking()
                on ur.IdRole equals r.Id
            where ur.IdUser == userId && r.IsActive
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
            where ur.IdUser == userId && r.IsActive && (f.IsActive ?? true)
            select f
        ).Distinct().ToListAsync(ct);

        // Legacy direct grants remain effective during migration.
        var legacyFunctions = await (
            from uf in _uow.Repository<F03UserFunction>().Query().AsNoTracking()
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on uf.Id equals f.Id
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
            IsActive = r.IsActive,
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
