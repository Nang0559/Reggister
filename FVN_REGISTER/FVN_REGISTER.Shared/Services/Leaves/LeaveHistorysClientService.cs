using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Contract.ViewModels.Leaves;


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

        public async Task<ApiResponse<List<LeaveRequestViewModel>>> GetHistoryAsync(
            int? year,
            string? status,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LEAVE_HISTORY_CLIENT] GetHistory start: year={Year}, status={Status}", year, status);

                // 1. Xây dựng URL Endpoint kèm Query String
                var url = "api/leavedays/history?";

                if (year.HasValue)
                    url += $"year={year.Value}&";

                if (!string.IsNullOrEmpty(status))
                    url += $"status={Uri.EscapeDataString(status)}&";

                // Xóa ký tự '&' hoặc '?' thừa ở cuối chuỗi URL
                url = url.TrimEnd('&').TrimEnd('?');

                // 2. Thực hiện gọi HTTP GET tới API
                var result = await _http.GetAsync<List<LeaveRequestViewModel>>(url, ct);

                _logger.LogDebugIf(Debug, "[LEAVE_HISTORY_CLIENT] GetHistory finished: success={Success}", result.IsSuccess);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_HISTORY_CLIENT] GetHistory error");
                return ApiResponse<List<LeaveRequestViewModel>>.Fail("Không thể tải lịch sử đơn nghỉ phép");
            }
        }
    }
}
