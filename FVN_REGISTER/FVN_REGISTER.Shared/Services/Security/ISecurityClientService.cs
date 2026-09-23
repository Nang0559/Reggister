using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Requests.Security;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Security;

public interface ISecurityClientService
{
    Task<ApiResponse<PermissionSnapshotDto>> GetMyPermissionsAsync(CancellationToken ct = default);
    Task<ApiResponse<List<SecurityRoleDto>>> GetRolesAsync(CancellationToken ct = default);
    Task<ApiResponse<List<SecurityFunctionDto>>> GetFunctionsAsync(CancellationToken ct = default);
    Task<ApiResponse<PermissionSnapshotDto>> GetUserPermissionsAsync(int userId, CancellationToken ct = default);
    Task<ApiResponse<PermissionSnapshotDto>> SetUserRolesAsync(int userId, List<int> roleCodes, CancellationToken ct = default);
    Task<ApiResponse<SecurityRoleDto>> SetRoleFunctionsAsync(int roleCode, List<int> functionCodes, CancellationToken ct = default);
    Task<ApiResponse<List<ManagedScopeDto>>> GetManagedScopesAsync(int userId, CancellationToken ct = default);
    Task<ApiResponse<PermissionSnapshotDto>> SetManagedScopesAsync(int userId, List<ManagedScopeRequest> scopes, CancellationToken ct = default);
    Task<ApiResponse<EffectivePermissionPreviewDto>> GetEffectivePermissionPreviewAsync(int userId, CancellationToken ct = default);
}
