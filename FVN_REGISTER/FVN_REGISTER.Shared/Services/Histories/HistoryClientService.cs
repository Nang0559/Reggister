using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Shared.Handlers;


namespace FVN_REGISTER.Shared.Services.Histories
{
    public class HistoryClientService : IHistoryClientService
    {
        private readonly IHttpClientWithAuth _http;

        public HistoryClientService(IHttpClientWithAuth http) => _http = http;

        private static string KindStr(RequestKind kind) =>
            kind == RequestKind.OT ? "ot" : "leave";

        public Task<ApiResponse<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, CancellationToken ct = default)
        {
            var kind = KindStr(filter.Kind);
            var url = $"api/History/{kind}?year={filter.Year}&status={filter.Status}" +
                      $"&fromDate={filter.FromDate:yyyy-MM-dd}&toDate={filter.ToDate:yyyy-MM-dd}" +
                      $"&search={filter.SearchText}&page={filter.Page}&pageSize={filter.PageSize}";
            return _http.GetPagedAsync<HistoryItemDto>(url, ct);
        }

        public Task<ApiResponse<ApprovalItemDetailDto>> GetDetailAsync(
            RequestKind kind, int id, CancellationToken ct = default)
            => _http.GetAsync<ApprovalItemDetailDto>($"api/History/{KindStr(kind)}/{id}", ct);

        public Task<ApiResponse<BalanceSummaryDto>> GetBalanceAsync(
            RequestKind kind, int year, CancellationToken ct = default)
            => _http.GetAsync<BalanceSummaryDto>($"api/History/{KindStr(kind)}/balance?year={year}", ct);

        public Task<ApiResponse<object>> CancelAsync(
            RequestKind kind, int id, string reason, CancellationToken ct = default)
            => _http.PostAsync<object>($"api/History/{KindStr(kind)}/{id}/cancel",
                new { Reason = reason }, ct);
    }
}
