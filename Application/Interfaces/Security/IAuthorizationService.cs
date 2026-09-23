using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Security;

namespace FVN_REGISTER.Application.Interfaces.Security;

public interface IAuthorizationService
{
    Task<bool> HasAsync(UserIdentityDto user, int functionCode, CancellationToken ct = default);
    Task<string> GetScopeAsync(int userId, int functionCode, CancellationToken ct = default);
    Task<bool> CanAccessAsync(UserIdentityDto user, int functionCode, string? employeeCode, string? deptCode, CancellationToken ct = default);
    Task<List<ManagedScopeDto>> GetManagedScopesAsync(int userId, CancellationToken ct = default);
    Task<List<ManagedEmployeeDto>> GetManagedEmployeesAsync(int userId, CancellationToken ct = default);
    Task<PermissionSnapshotDto> ReplaceManagedScopesAsync(int userId, IReadOnlyCollection<ManagedScopeRequest> scopes, int actorUserId, CancellationToken ct = default);
    Task<EffectivePermissionPreviewDto> GetEffectivePermissionPreviewAsync(int userId, CancellationToken ct = default);
    Task<PermissionSnapshotDto> GetSnapshotAsync(int userId, CancellationToken ct = default);
    Task<List<SecurityRoleDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<SecurityFunctionDto>> GetFunctionsAsync(CancellationToken ct = default);
    Task<PermissionSnapshotDto> SetUserRolesAsync(int userId, IReadOnlyCollection<int> roleCodes, int actorUserId, CancellationToken ct = default);
    Task<SecurityRoleDto> SetRoleFunctionsAsync(int roleCode, IReadOnlyCollection<int> functionCodes, int actorUserId, CancellationToken ct = default);
}
