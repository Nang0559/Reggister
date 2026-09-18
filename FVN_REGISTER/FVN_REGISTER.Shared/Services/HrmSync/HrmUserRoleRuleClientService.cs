using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Requests.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.HrmSync;

public sealed class HrmUserRoleRuleClientService : IHrmUserRoleRuleClientService
{
    private readonly IHttpClientWithAuth _http;
    private const string Base = "api/hrm-role-rules";

    public HrmUserRoleRuleClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<List<HrmUserRoleRuleDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetAsync<List<HrmUserRoleRuleDto>>(Base, ct);

    public Task<ApiResponse<HrmUserRoleRuleDto>> CreateAsync(HrmUserRoleRuleRequest request, CancellationToken ct = default)
        => _http.PostAsync<HrmUserRoleRuleDto>(Base, request, ct);

    public Task<ApiResponse<HrmUserRoleRuleDto>> UpdateAsync(int id, HrmUserRoleRuleRequest request, CancellationToken ct = default)
        => _http.PutAsync<HrmUserRoleRuleDto>($"{Base}/{id}", request, ct);

    public Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteAsync<object>($"{Base}/{id}", ct);
}