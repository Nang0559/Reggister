using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Services.Histories;

namespace FVN_REGISTER.Shared.Services.History
{
    public class HistoryClientService : IHistoryClientService
    {
        private readonly IHttpClientWithAuth _http;

        public HistoryClientService(IHttpClientWithAuth http)
        {
            _http = http;
        }

        private static string KindStr(RequestModule kind)
            => kind.ToCode().ToLowerInvariant();

        public Task<ApiResponse<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter,
            CancellationToken ct = default)
        {
            var kind = KindStr(filter.Kind);

            var url =
                $"api/History/{kind}" +
                $"?year={filter.Year}" +
                $"&status={Uri.EscapeDataString(filter.Status ?? string.Empty)}" +
                $"&fromDate={filter.FromDate:yyyy-MM-dd}" +
                $"&toDate={filter.ToDate:yyyy-MM-dd}" +
                $"&search={Uri.EscapeDataString(filter.SearchText ?? string.Empty)}" +
                $"&page={filter.Page}" +
                $"&pageSize={filter.PageSize}";

            return _http.GetPagedAsync<HistoryItemDto>(url, ct);
        }

        public Task<ApiResponse<HistoryItemDetailDto>> GetDetailAsync(
            RequestModule kind, int id, CancellationToken ct = default)
            => _http.GetAsync<HistoryItemDetailDto>(
                $"api/History/{KindStr(kind)}/{id}", ct);

        public Task<ApiResponse<BalanceSummaryDto>> GetBalanceAsync(
            RequestModule kind,
            int year,
            CancellationToken ct = default)
        {
            var url =
                $"api/History/{KindStr(kind)}/balance?year={year}";

            return _http.GetAsync<BalanceSummaryDto>(url, ct);
        }

        public Task<ApiResponse<object>> CancelAsync(
            RequestModule kind,
            int id,
            string reason,
            CancellationToken ct = default)
        {
            var url =
                $"api/History/{KindStr(kind)}/{id}/cancel";

            return _http.PostAsync<object>(
                url,
                new HistoryCancelRequestDto
                {
                    Reason = reason
                },
                ct);
        }
    }
}