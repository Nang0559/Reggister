using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Requests.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Security;

public sealed class SecurityClientService : ISecurityClientService
{
    private readonly IHttpClientWithAuth _http;

    public SecurityClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<PermissionSnapshotDto>> GetMyPermissionsAsync(CancellationToken ct = default)
        => _http.GetAsync<PermissionSnapshotDto>("api/security/me", ct);

    public Task<ApiResponse<List<SecurityRoleDto>>> GetRolesAsync(CancellationToken ct = default)
        => _http.GetAsync<List<SecurityRoleDto>>("api/security/roles", ct);

    public Task<ApiResponse<List<SecurityFunctionDto>>> GetFunctionsAsync(CancellationToken ct = default)
        => _http.GetAsync<List<SecurityFunctionDto>>("api/security/functions", ct);

    public Task<ApiResponse<PermissionSnapshotDto>> GetUserPermissionsAsync(int userId, CancellationToken ct = default)
        => _http.GetAsync<PermissionSnapshotDto>($"api/security/users/{userId}/permissions", ct);

    public Task<ApiResponse<PermissionSnapshotDto>> SetUserRolesAsync(int userId, List<int> roleCodes, CancellationToken ct = default)
        => _http.PutAsync<PermissionSnapshotDto>(
            $"api/security/users/{userId}/roles",
            new UpdateUserRolesRequest { UserId = userId, RoleCodes = roleCodes },
            ct);
    
    public Task<ApiResponse<SecurityRoleDto>> SetRoleFunctionsAsync(
        int roleCode, List<int> functionCodes, CancellationToken ct = default)
        => _http.PutAsync<SecurityRoleDto>(
            $"api/security/roles/{roleCode}/functions",
            new UpdateRoleFunctionsRequest { RoleCode = roleCode, FunctionCodes = functionCodes },
            ct);
}
