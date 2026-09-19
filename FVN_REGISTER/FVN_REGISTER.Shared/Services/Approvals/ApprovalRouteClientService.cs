using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Approvals;

public sealed class ApprovalRouteClientService : IApprovalRouteClientService
{
    private readonly IHttpClientWithAuth _http;

    public ApprovalRouteClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<ApprovalRoutePreviewDto>> GetPreviewAsync(
        RequestModule requestType,
        CancellationToken ct = default)
        => _http.GetAsync<ApprovalRoutePreviewDto>(
            $"api/approval-route/preview?requestType={Uri.EscapeDataString(requestType.ToString())}", ct);
}
