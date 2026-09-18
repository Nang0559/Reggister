using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.HrmSync;

public sealed class HrmSyncClientService : IHrmSyncClientService
{
    private readonly IHttpClientWithAuth _http;
    private const string Base = "api/hrm-sync";

    public HrmSyncClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<HrmSyncRuntimeStatusDto>> GetStatusAsync(CancellationToken ct = default)
        => _http.GetAsync<HrmSyncRuntimeStatusDto>($"{Base}/status", ct);

    public Task<ApiResponse<HrmSyncRunResultDto>> RunAllAsync(CancellationToken ct = default)
        => _http.PostAsync<HrmSyncRunResultDto>($"{Base}/run", new { }, ct);

    public Task<ApiResponse<HrmSyncRunResultDto>> RunEntityAsync(string entityType, CancellationToken ct = default)
        => _http.PostAsync<HrmSyncRunResultDto>($"{Base}/run/{Uri.EscapeDataString(entityType)}", new { }, ct);
}
