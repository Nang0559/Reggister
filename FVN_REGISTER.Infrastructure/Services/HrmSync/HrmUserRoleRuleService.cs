using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Requests.HrmSync;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync;

public sealed class HrmUserRoleRuleService : IHrmUserRoleRuleService
{
    private readonly IUnitOfWork _uow;

    public HrmUserRoleRuleService(IUnitOfWork uow) => _uow = uow;

    public async Task<ServiceResult<List<HrmUserRoleRuleDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var rules = await _uow.Repository<F03HrmUserRoleRule>().Query()
            .AsNoTracking()
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var depts = await _uow.Repository<F03Department>().Query().AsNoTracking().ToListAsync(ct);
        var positions = await _uow.Repository<F03Position>().Query().AsNoTracking().ToListAsync(ct);
        var permissions = await _uow.Repository<F03Permission>().Query().AsNoTracking().ToListAsync(ct);

        var deptMap = depts.ToDictionary(x => x.DeptCode, x => x.DeptName);
        var positionMap = positions.ToDictionary(x => x.PositionCode, x => x.PositionName);
        var permissionMap = permissions.ToDictionary(x => x.PermissionCode, x => x.PermissionName);

        return ServiceResult<List<HrmUserRoleRuleDto>>.Ok(rules.Select(x => new HrmUserRoleRuleDto
        {
            Id = x.Id,
            IsActive = x.IsActive,
            DeptCode = x.DeptCode,
            DeptName = x.DeptCode == null ? "Tất cả phòng ban" : deptMap.GetValueOrDefault(x.DeptCode),
            PositionCode = x.PositionCode,
            PositionName = x.PositionCode == null ? "Tất cả chức vụ" : positionMap.GetValueOrDefault(x.PositionCode),
            PermissionCode = x.PermissionCode,
            PermissionName = permissionMap.GetValueOrDefault(x.PermissionCode),
            Priority = x.Priority,
            Note = x.Note
        }).ToList());
    }

    public async Task<ServiceResult<HrmUserRoleRuleDto>> CreateAsync(HrmUserRoleRuleRequest request, int actorUserId, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(request, null, ct);
        if (validation != null) return ServiceResult<HrmUserRoleRuleDto>.Fail(validation);

        var entity = new F03HrmUserRoleRule
        {
            DeptCode = Normalize(request.DeptCode),
            PositionCode = Normalize(request.PositionCode),
            PermissionCode = request.PermissionCode,
            Priority = request.Priority,
            Note = Normalize(request.Note),
            IsActive = request.IsActive,
            CreatedBy = actorUserId,
            LastModifiedSource = "Manual"
        };

        await _uow.Repository<F03HrmUserRoleRule>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        await ReconcileUsersAsync(actorUserId, ct);
        return ServiceResult<HrmUserRoleRuleDto>.Ok(await MapAsync(entity, ct));
    }

    public async Task<ServiceResult<HrmUserRoleRuleDto>> UpdateAsync(int id, HrmUserRoleRuleRequest request, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03HrmUserRoleRule>().Query().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return ServiceResult<HrmUserRoleRuleDto>.Fail("Không tìm thấy rule.");

        var validation = await ValidateAsync(request, id, ct);
        if (validation != null) return ServiceResult<HrmUserRoleRuleDto>.Fail(validation);

        entity.DeptCode = Normalize(request.DeptCode);
        entity.PositionCode = Normalize(request.PositionCode);
        entity.PermissionCode = request.PermissionCode;
        entity.Priority = request.Priority;
        entity.Note = Normalize(request.Note);
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        entity.LastModifiedSource = "Manual";

        await _uow.SaveChangesAsync(ct);
        await ReconcileUsersAsync(actorUserId, ct);
        return ServiceResult<HrmUserRoleRuleDto>.Ok(await MapAsync(entity, ct));
    }

    public async Task<ServiceResult<object>> DeleteAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03HrmUserRoleRule>().Query().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return ServiceResult<object>.Fail("Không tìm thấy rule.");

        entity.IsActive = false;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        entity.LastModifiedSource = "Manual";
        await _uow.SaveChangesAsync(ct);
        await ReconcileUsersAsync(actorUserId, ct);
        return ServiceResult<object>.Ok(new { id, deactivated = true });
    }

    private async Task ReconcileUsersAsync(int actorUserId, CancellationToken ct)
    {
        var rules = await _uow.Repository<F03HrmUserRoleRule>().Query()
            .AsNoTracking()
             .Where(x => x.IsActive == true)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var users = await _uow.Repository<F03User>().Query()
            .Where(x => x.IsActive == true && x.LastModifiedSource == SyncSourceTags.Hrm)
            .ToListAsync(ct);

        foreach (var user in users)
        {
            var rule = rules
                .Where(x => (x.DeptCode == null || x.DeptCode == user.DeptCode) &&
                            (x.PositionCode == null || x.PositionCode == user.Cvcode))
                .OrderBy(x => x.DeptCode == user.DeptCode && x.PositionCode == user.Cvcode ? 0 :
                               x.DeptCode == user.DeptCode && x.PositionCode == null ? 1 :
                               x.DeptCode == null && x.PositionCode == user.Cvcode ? 2 : 3)
                .ThenBy(x => x.Priority)
                .ThenBy(x => x.Id)
                .FirstOrDefault();

            var permission = rule?.PermissionCode ?? UserPermissionCodes.User;
            if (user.PermissionCode != permission)
            {
                user.PermissionCode = permission;
                user.ModifiedBy = actorUserId;
                user.ModifiedAt = DateTime.Now;
                // Đây vẫn là tài khoản do HRM provisioning sở hữu. Thay đổi role rule
                // không được chuyển ownership sang Manual, nếu không các lần reconcile
                // HRM sau đó sẽ bỏ qua tài khoản này.
                user.LastModifiedSource = SyncSourceTags.Hrm;
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    private async Task<string?> ValidateAsync(HrmUserRoleRuleRequest request, int? excludeId, CancellationToken ct)
    {
        if (request.PermissionCode is < 1 or > 6) return "PermissionCode không hợp lệ.";
        if (request.Priority < 0) return "Priority phải >= 0.";

        var dept = Normalize(request.DeptCode);
        var position = Normalize(request.PositionCode);

        var duplicate = await _uow.Repository<F03HrmUserRoleRule>().Query()
            .AnyAsync(x => x.IsActive == true &&
                           x.Id != excludeId &&
                           x.DeptCode == dept &&
                           x.PositionCode == position &&
                           x.PermissionCode == request.PermissionCode, ct);

        return duplicate ? "Rule trùng Department + Position + PermissionCode đang active." : null;
    }

    private async Task<HrmUserRoleRuleDto> MapAsync(F03HrmUserRoleRule x, CancellationToken ct)
    {
        var dept = x.DeptCode == null ? null : await _uow.Repository<F03Department>().Query().AsNoTracking().FirstOrDefaultAsync(d => d.DeptCode == x.DeptCode, ct);
        var pos = x.PositionCode == null ? null : await _uow.Repository<F03Position>().Query().AsNoTracking().FirstOrDefaultAsync(p => p.PositionCode == x.PositionCode, ct);
        var perm = await _uow.Repository<F03Permission>().Query().AsNoTracking().FirstOrDefaultAsync(p => p.PermissionCode == x.PermissionCode, ct);

        return new HrmUserRoleRuleDto
        {
            Id = x.Id, IsActive = x.IsActive, DeptCode = x.DeptCode, DeptName = dept?.DeptName ?? (x.DeptCode == null ? "Tất cả phòng ban" : null),
            PositionCode = x.PositionCode, PositionName = pos?.PositionName ?? (x.PositionCode == null ? "Tất cả chức vụ" : null),
            PermissionCode = x.PermissionCode, PermissionName = perm?.PermissionName, Priority = x.Priority, Note = x.Note
        };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}