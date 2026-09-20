using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.OTs;

public sealed class OTLimitRuleManagementClientService : IOTLimitRuleManagementClientService
{
    private const string BaseUrl = "api/admin/ot-limit-rules";
    private readonly IHttpClientWithAuth _http;

    public OTLimitRuleManagementClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<List<OTLimitRuleDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetAsync<List<OTLimitRuleDto>>(BaseUrl, ct);

    public Task<ApiResponse<OTLimitRuleDto>> GetByIdAsync(int id, CancellationToken ct = default)
        => _http.GetAsync<OTLimitRuleDto>($"{BaseUrl}/{id}", ct);

    public Task<ApiResponse<object>> CreateAsync(OTLimitRuleUpsertDto model, CancellationToken ct = default)
        => _http.PostAsync<object>(BaseUrl, model, ct);

    public Task<ApiResponse<object>> UpdateAsync(int id, OTLimitRuleUpsertDto model, CancellationToken ct = default)
        => _http.PutAsync<object>($"{BaseUrl}/{id}", model, ct);

    public Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default)
        => _http.PatchAsync<object>($"{BaseUrl}/{id}/toggle", new { }, ct);

    public Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteAsync<object>($"{BaseUrl}/{id}", ct);
}
