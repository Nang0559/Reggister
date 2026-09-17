using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.HrmSync;

public sealed class HrmSyncReviewClientService : IHrmSyncReviewClientService
{
    private readonly IHttpClientWithAuth _http;
    private const string Base = "api/hrm-sync/review";

    public HrmSyncReviewClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<List<SyncReviewFlagDto>>> GetUnresolvedAsync(string? entityType = null, CancellationToken ct = default)
        => _http.GetAsync<List<SyncReviewFlagDto>>(
            string.IsNullOrWhiteSpace(entityType) ? Base : $"{Base}?entityType={Uri.EscapeDataString(entityType)}", ct);

    public Task<ApiResponse<int>> CountAsync(CancellationToken ct = default)
        => _http.GetAsync<int>($"{Base}/count", ct);

    public Task<ApiResponse<object>> ResolveAsync(int flagId, CancellationToken ct = default)
        => _http.PostAsync<object>($"{Base}/{flagId}/resolve", new { }, ct);

    public Task<ApiResponse<object>> ResolveByEntityAsync(string entityType, string entityKey, CancellationToken ct = default)
        => _http.PostAsync<object>($"{Base}/resolve-by-entity?entityType={Uri.EscapeDataString(entityType)}&entityKey={Uri.EscapeDataString(entityKey)}", new { }, ct);
}
