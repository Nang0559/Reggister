using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
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
    Task<ApiResponse<List<ManagedEmployeeDto>>> GetManagedEmployeesAsync(int userId, CancellationToken ct = default);
    Task<ApiResponse<List<ManagedEmployeeDto>>> GetMyManagedEmployeesAsync(CancellationToken ct = default);
    Task<ApiResponse<EffectivePermissionPreviewDto>> GetEffectivePermissionPreviewAsync(int userId, CancellationToken ct = default);
    Task<ApiResponse<List<FeatureOperatorAssignmentDto>>> GetFeatureOperatorsAsync(int functionCode, string resourceType, int? resourceId = null, CancellationToken ct = default);
    Task<ApiResponse<List<FeatureOperatorEmployeeDto>>> GetFeatureOperatorEmployeesAsync(string? search = null, CancellationToken ct = default);
    Task<ApiResponse<List<FeatureOperatorResourceDto>>> GetFeatureOperatorResourcesAsync(string resourceType, CancellationToken ct = default);
    Task<ApiResponse<FeatureOperatorAssignmentDto>> AddFeatureOperatorAsync(SaveFeatureOperatorAssignmentRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> RemoveFeatureOperatorAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<List<TwoFactorAdminUserDto>>> GetTwoFactorUsersAsync(CancellationToken ct = default);
    Task<ApiResponse<object>> SetTwoFactorRequiredAsync(int userId, bool required, CancellationToken ct = default);
    Task<ApiResponse<object>> ResetTwoFactorAsync(int userId, CancellationToken ct = default);
    Task<ApiResponse<List<SecurityAuditEntryDto>>> GetSecurityAuditAsync(DateTime? from = null, DateTime? to = null, string? search = null, CancellationToken ct = default);
    Task<ApiResponse<SecurityFunctionDiscoverySummaryDto>> ScanSecurityFunctionsAsync(CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<SecurityFunctionRegistryItemDto>>> GetSecurityFunctionRegistryAsync(string? status = null, CancellationToken ct = default);
    Task<ApiResponse<object>> RegisterSecurityFunctionAsync(string functionKey, RegisterDiscoveredFunctionRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> RetireSecurityFunctionAsync(string functionKey, CancellationToken ct = default);
    Task<ApiResponse<object>> ReplaceSecurityFunctionAsync(string functionKey, ReplaceFunctionRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> IgnoreSecurityFunctionAsync(string functionKey, CancellationToken ct = default);
    Task<ApiResponse<object>> UpsertSecurityFunctionAsync(SecurityFunctionUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> DeleteSecurityFunctionAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<object>> UpsertSecurityRoleAsync(SecurityRoleUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> DeleteSecurityRoleAsync(int id, CancellationToken ct = default);
}
