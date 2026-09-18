using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Security;

public sealed class AuthorizationService : BaseService<AuthorizationService>, IAuthorizationService
{
    private readonly IUnitOfWork _uow;

    public AuthorizationService(
        IUnitOfWork uow,
        ILogger<AuthorizationService> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _uow = uow;
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

    public async Task<PermissionSnapshotDto> GetSnapshotAsync(
        int userId,
        CancellationToken ct = default)
    {
        var roleCodes = await (
            from ur in _uow.Repository<F03UserRole>().Query().AsNoTracking()
            join r in _uow.Repository<F03Role>().Query().AsNoTracking()
                on ur.IdRole equals r.IdRole
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
                on rf.IdFunction equals f.IdFunction
            join r in _uow.Repository<F03Role>().Query().AsNoTracking()
                on ur.IdRole equals r.IdRole
            where ur.IdUser == userId && r.IsActive && (f.IsActive ?? true)
            select f
        ).Distinct().ToListAsync(ct);

        // Legacy direct grants remain effective during migration.
        var legacyFunctions = await (
            from uf in _uow.Repository<F03UserFunction>().Query().AsNoTracking()
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on uf.IdFunction equals f.IdFunction
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
                IdFunction = x.IdFunction,
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
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoleCode)
            .ToListAsync(ct);

        var roleIds = roles.Select(x => x.IdRole).ToList();
        var map = await (
            from rf in _uow.Repository<F03RoleFunction>().Query().AsNoTracking()
            join f in _uow.Repository<F03Function>().Query().AsNoTracking()
                on rf.IdFunction equals f.IdFunction
            where roleIds.Contains(rf.IdRole)
            select new { rf.IdRole, f.FunctionCode }
        ).ToListAsync(ct);

        return roles.Select(r => new SecurityRoleDto
        {
            IdRole = r.IdRole,
            RoleCode = r.RoleCode,
            RoleName = r.RoleName,
            Detail = r.Detail,
            IsSystem = r.IsSystem,
            IsActive = r.IsActive,
            FunctionCodes = map.Where(x => x.IdRole == r.IdRole)
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
                IdFunction = x.IdFunction,
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

        var roles = await _uow.Repository<F03Role>().Query()
            .Where(x => x.IsActive && roleCodes.Contains(x.RoleCode))
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
                IdRole = role.IdRole,
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
        return await GetSnapshotAsync(userId, ct);
    }
}
