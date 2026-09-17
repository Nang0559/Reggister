using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class LeaveHistorysClientService : ILeaveHistorysClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<LeaveHistorysClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public LeaveHistorysClientService(
            IHttpClientWithAuth http,
            ILogger<LeaveHistorysClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public async Task<ApiResponse<PaginationResult<LeaveSummaryDto>>> GetHistoryAsync(
            int? year,
            string? status,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                var query = new List<string>
                {
                    $"page={Math.Max(1, page)}",
                    $"pageSize={Math.Clamp(pageSize, 1, 100)}"
                };

                if (year.HasValue)
                    query.Add($"year={year.Value}");

                if (!string.IsNullOrWhiteSpace(status))
                    query.Add($"status={Uri.EscapeDataString(status)}");

                var url = $"api/leavedays/history?{string.Join("&", query)}";
                _logger.LogDebugIf(Debug, "[LEAVE_HISTORY_CLIENT] GET {Url}", url);

                return await _http.GetAsync<PaginationResult<LeaveSummaryDto>>(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_HISTORY_CLIENT] GetHistory error");
                return ApiResponse<PaginationResult<LeaveSummaryDto>>.Fail("Không thể tải lịch sử đơn nghỉ phép");
            }
        }
    }
}
