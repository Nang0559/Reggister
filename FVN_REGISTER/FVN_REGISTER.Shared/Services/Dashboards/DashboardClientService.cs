
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Shared.Services.Dashboards
{
    public class DashboardClientService : IDashboardClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<DashboardClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public DashboardClientService(
            IHttpClientWithAuth http,
            ILogger<DashboardClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        // ================= GET DASHBOARD =================
        public async Task<ApiResponse<LeaveDashboardViewModel>> GetDashboardDataAsync(CancellationToken ct = default)
        {
            try
            {
                // Thêm "Debug" vào tham số thứ nhất
                _logger.LogDebugIf(Debug, "[DASHBOARD] Fetch start");

                var result = await _http.GetAsync<LeaveDashboardViewModel>("api/Dashboard", ct);

                if (result.IsSuccess)
                {
                    _logger.LogDebugIf(Debug, "[DASHBOARD] Fetch success");
                }
                else
                {
                    // Thêm "Debug" vào đây nữa
                    _logger.LogWarnIf(Debug, "[DASHBOARD] Fetch failed | Status={Status}", result.StatusCode);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DASHBOARD] Exception while fetching data");
                return ApiResponse<LeaveDashboardViewModel>.Fail("Không thể tải dashboard.");
            }
        }
    }
}
