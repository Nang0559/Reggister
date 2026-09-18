using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Dashboards
{
    public sealed class DashboardClientService : IDashboardClientService
    {
        private const string BaseUrl = "api/dashboard";
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<DashboardClientService> _logger;

        public DashboardClientService(
            IHttpClientWithAuth http,
            ILogger<DashboardClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ApiResponse<DashboardResponse>> GetDashboardDataAsync(
            CancellationToken ct = default)
        {
            try
            {
                var result = await _http.GetAsync<DashboardResponse>(BaseUrl, ct);
                if (!result.IsSuccess)
                    _logger.LogWarning("[DASHBOARD] Fetch failed | Status={Status}", result.StatusCode);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DASHBOARD] Exception while fetching data");
                return ApiResponse<DashboardResponse>.Fail("Không thể tải dashboard.");
            }
        }
    }
}
