using FVN_REGISTER.Contract.Dtos.Security;

namespace FVN_REGISTER.Application.Interfaces.Security;

/// <summary>
/// Application boundary for discovering and managing the security-function catalog.
/// Implementations must not grant or revoke business permissions during discovery.
/// </summary>
public interface ISecurityFunctionRegistryService
{
    Task<SecurityFunctionDiscoverySummaryDto> ReconcileAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SecurityFunctionRegistryItemDto>> GetRegistryAsync(string? status = null, CancellationToken ct = default);
    Task<SecurityFunctionRegistryItemDto?> GetAsync(string functionKey, CancellationToken ct = default);
    Task RegisterAsync(string functionKey, RegisterDiscoveredFunctionRequest request, int actorUserId, CancellationToken ct = default);
    Task RetireAsync(string functionKey, int actorUserId, CancellationToken ct = default);
    Task ReplaceAsync(string functionKey, ReplaceFunctionRequest request, int actorUserId, CancellationToken ct = default);
    Task IgnoreAsync(string functionKey, int actorUserId, CancellationToken ct = default);
    Task UpsertFunctionAsync(SecurityFunctionUpsertRequest request, int actorUserId, CancellationToken ct = default);
    Task DeleteFunctionAsync(int id, int actorUserId, CancellationToken ct = default);
    Task UpsertRoleAsync(SecurityRoleUpsertRequest request, int actorUserId, CancellationToken ct = default);
    Task DeleteRoleAsync(int id, int actorUserId, CancellationToken ct = default);
}
