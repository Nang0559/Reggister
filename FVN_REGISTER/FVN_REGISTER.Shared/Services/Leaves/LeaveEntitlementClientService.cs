using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Leaves;

public sealed class LeaveEntitlementClientService : ILeaveEntitlementClientService
{
    private readonly IHttpClientWithAuth _http;

    public LeaveEntitlementClientService(IHttpClientWithAuth http)
    {
        _http = http;
    }

    public Task<ApiResponse<LeaveEntitlementDto>> GetMineAsync(
        int? year = null,
        CancellationToken ct = default)
    {
        var suffix = year.HasValue ? $"?year={year.Value}" : string.Empty;
        return _http.GetAsync<LeaveEntitlementDto>(
            $"api/leave-entitlements/me{suffix}",
            ct);
    }
}
