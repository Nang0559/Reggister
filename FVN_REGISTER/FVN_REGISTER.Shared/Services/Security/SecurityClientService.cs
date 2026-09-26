using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Security;

public sealed class SecurityClientService : ISecurityClientService
{
    private readonly IHttpClientWithAuth _http;
    public SecurityClientService(IHttpClientWithAuth http) => _http = http;
    public Task<ApiResponse<PermissionSnapshotDto>> GetMyPermissionsAsync(CancellationToken ct = default) => _http.GetAsync<PermissionSnapshotDto>("api/security/me", ct);
    public Task<ApiResponse<List<SecurityRoleDto>>> GetRolesAsync(CancellationToken ct = default) => _http.GetAsync<List<SecurityRoleDto>>("api/security/roles", ct);
    public Task<ApiResponse<List<SecurityFunctionDto>>> GetFunctionsAsync(CancellationToken ct = default) => _http.GetAsync<List<SecurityFunctionDto>>("api/security/functions", ct);
    public Task<ApiResponse<PermissionSnapshotDto>> GetUserPermissionsAsync(int userId, CancellationToken ct = default) => _http.GetAsync<PermissionSnapshotDto>($"api/security/users/{userId}/permissions", ct);
    public Task<ApiResponse<PermissionSnapshotDto>> SetUserRolesAsync(int userId, List<int> roleCodes, CancellationToken ct = default) => _http.PutAsync<PermissionSnapshotDto>($"api/security/users/{userId}/roles", new UpdateUserRolesRequest { UserId = userId, RoleCodes = roleCodes }, ct);
    public Task<ApiResponse<List<ManagedScopeDto>>> GetManagedScopesAsync(int userId, CancellationToken ct = default) => _http.GetAsync<List<ManagedScopeDto>>($"api/security/users/{userId}/managed-scopes", ct);
    public Task<ApiResponse<PermissionSnapshotDto>> SetManagedScopesAsync(int userId, List<ManagedScopeRequest> scopes, CancellationToken ct = default) => _http.PutAsync<PermissionSnapshotDto>($"api/security/users/{userId}/managed-scopes", new UpdateManagedScopesRequest { UserId = userId, Scopes = scopes }, ct);
    public Task<ApiResponse<EffectivePermissionPreviewDto>> GetEffectivePermissionPreviewAsync(int userId, CancellationToken ct = default) => _http.GetAsync<EffectivePermissionPreviewDto>($"api/security/users/{userId}/effective-permission", ct);
    public Task<ApiResponse<List<ManagedEmployeeDto>>> GetManagedEmployeesAsync(int userId, CancellationToken ct = default) => _http.GetAsync<List<ManagedEmployeeDto>>($"api/security/users/{userId}/managed-employees", ct);
    public Task<ApiResponse<List<ManagedEmployeeDto>>> GetMyManagedEmployeesAsync(CancellationToken ct = default) => _http.GetAsync<List<ManagedEmployeeDto>>("api/security/me/managed-employees", ct);
    public Task<ApiResponse<List<FeatureOperatorAssignmentDto>>> GetFeatureOperatorsAsync(int functionCode, string resourceType, int? resourceId = null, CancellationToken ct = default) => _http.GetAsync<List<FeatureOperatorAssignmentDto>>($"api/security/feature-operators?functionCode={functionCode}&resourceType={Uri.EscapeDataString(resourceType)}{(resourceId.HasValue ? $"&resourceId={resourceId.Value}" : string.Empty)}", ct);
    public Task<ApiResponse<List<FeatureOperatorResourceDto>>> GetFeatureOperatorResourcesAsync(string resourceType, CancellationToken ct = default) => _http.GetAsync<List<FeatureOperatorResourceDto>>($"api/security/feature-operators/resources?resourceType={Uri.EscapeDataString(resourceType)}", ct);
    public Task<ApiResponse<List<FeatureOperatorEmployeeDto>>> GetFeatureOperatorEmployeesAsync(string? search = null, CancellationToken ct = default) => _http.GetAsync<List<FeatureOperatorEmployeeDto>>($"api/security/feature-operators/employees{(string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}")}", ct);
    public Task<ApiResponse<FeatureOperatorAssignmentDto>> AddFeatureOperatorAsync(SaveFeatureOperatorAssignmentRequest request, CancellationToken ct = default) => _http.PostAsync<FeatureOperatorAssignmentDto>("api/security/feature-operators", request, ct);
    public Task<ApiResponse<object>> RemoveFeatureOperatorAsync(int id, CancellationToken ct = default) => _http.DeleteAsync<object>($"api/security/feature-operators/{id}", ct);
    public Task<ApiResponse<SecurityRoleDto>> SetRoleFunctionsAsync(int roleCode, List<int> functionCodes, CancellationToken ct = default) => _http.PutAsync<SecurityRoleDto>($"api/security/roles/{roleCode}/functions", new UpdateRoleFunctionsRequest { RoleCode = roleCode, FunctionCodes = functionCodes }, ct);
    public Task<ApiResponse<List<TwoFactorAdminUserDto>>> GetTwoFactorUsersAsync(CancellationToken ct = default) => _http.GetAsync<List<TwoFactorAdminUserDto>>("api/security/users/2fa", ct);
    public Task<ApiResponse<object>> SetTwoFactorRequiredAsync(int userId, bool required, CancellationToken ct = default) => _http.PutAsync<object>($"api/security/users/{userId}/2fa-required", new TwoFactorRequirementRequest { Required = required }, ct);
    public Task<ApiResponse<object>> ResetTwoFactorAsync(int userId, CancellationToken ct = default) => _http.PostAsync<object>($"api/security/users/{userId}/2fa/reset", new { }, ct);
    public async Task<ApiResponse<List<SecurityAuditEntryDto>>> GetSecurityAuditAsync(DateTime? from = null, DateTime? to = null, string? search = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (from.HasValue) query.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
        if (to.HasValue) query.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        var url = "api/security/audit" + (query.Count == 0 ? "" : "?" + string.Join("&", query));
        return await _http.GetAsync<List<SecurityAuditEntryDto>>(url, ct);
    }
    public Task<ApiResponse<SecurityFunctionDiscoverySummaryDto>> ScanSecurityFunctionsAsync(CancellationToken ct = default) => _http.PostAsync<SecurityFunctionDiscoverySummaryDto>("api/security/registry/scan", new { }, ct);
    public Task<ApiResponse<IReadOnlyList<SecurityFunctionRegistryItemDto>>> GetSecurityFunctionRegistryAsync(string? status = null, CancellationToken ct = default) => _http.GetAsync<IReadOnlyList<SecurityFunctionRegistryItemDto>>("api/security/registry/functions" + (string.IsNullOrWhiteSpace(status) ? string.Empty : $"?status={Uri.EscapeDataString(status)}"), ct);
    public Task<ApiResponse<WebSecurityManifestSummaryDto>> PublishWebSecurityManifestAsync(IReadOnlyList<WebSecurityFunctionCandidateDto> entries, CancellationToken ct = default) => _http.PostAsync<WebSecurityManifestSummaryDto>("api/security/registry/web-manifest", new WebSecurityManifestRequest(entries), ct);
    public Task<ApiResponse<object>> RegisterSecurityFunctionAsync(string functionKey, RegisterDiscoveredFunctionRequest request, CancellationToken ct = default) => _http.PostAsync<object>($"api/security/registry/functions/{Uri.EscapeDataString(functionKey)}/register", request, ct);
    public Task<ApiResponse<object>> RetireSecurityFunctionAsync(string functionKey, CancellationToken ct = default) => _http.PostAsync<object>($"api/security/registry/functions/{Uri.EscapeDataString(functionKey)}/retire", new { }, ct);
    public Task<ApiResponse<object>> ReplaceSecurityFunctionAsync(string functionKey, ReplaceFunctionRequest request, CancellationToken ct = default) => _http.PostAsync<object>($"api/security/registry/functions/{Uri.EscapeDataString(functionKey)}/replace", request, ct);
    public Task<ApiResponse<object>> IgnoreSecurityFunctionAsync(string functionKey, CancellationToken ct = default) => _http.PostAsync<object>($"api/security/registry/functions/{Uri.EscapeDataString(functionKey)}/ignore", new { }, ct);
    public Task<ApiResponse<object>> UpsertSecurityFunctionAsync(SecurityFunctionUpsertRequest request, CancellationToken ct = default) => _http.PostAsync<object>("api/security/registry/functions", request, ct);
    public Task<ApiResponse<object>> DeleteSecurityFunctionAsync(int id, CancellationToken ct = default) => _http.DeleteAsync<object>($"api/security/registry/functions/{id}", ct);
    public Task<ApiResponse<object>> UpsertSecurityRoleAsync(SecurityRoleUpsertRequest request, CancellationToken ct = default) => _http.PostAsync<object>("api/security/registry/roles", request, ct);
    public Task<ApiResponse<object>> DeleteSecurityRoleAsync(int id, CancellationToken ct = default) => _http.DeleteAsync<object>($"api/security/registry/roles/{id}", ct);
}
