using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public class OTSyncClientService : IOTSyncClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<OTSyncClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public OTSyncClientService(
            IHttpClientWithAuth http,
            ILogger<OTSyncClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public async Task<ApiResponse<OTSyncResultDto>> SyncAsync(
            DateTime date,
            string? deptCode = null,
            CancellationToken ct = default)
        {
            try
            {
                var url = BuildUrl("api/OTSync/sync", date, deptCode);
                _logger.LogInfoIf(Debug, "[OT_CLIENT] Sync → {Url}", url);
                return await _http.PostAsync<OTSyncResultDto>(url, new { }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] SyncAsync lỗi");
                return ApiResponse<OTSyncResultDto>.Fail("Không thể kết nối server.");
            }
        }

        public async Task<ApiResponse<OTUnconfirmedListResponse>> GetUnconfirmedAsync(
            DateTime date,
            string? deptCode = null,
            CancellationToken ct = default)
        {
            try
            {
                var url = BuildUrl("api/OTSync/unconfirmed", date, deptCode);
                _logger.LogInfoIf(Debug, "[OT_CLIENT] GetUnconfirmed → {Url}", url);
                return await _http.GetAsync<OTUnconfirmedListResponse>(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetUnconfirmedAsync lỗi");
                return ApiResponse<OTUnconfirmedListResponse>.Fail("Không thể kết nối server.");
            }
        }

        public async Task<ApiResponse<OTWorkerStatusDto>> GetWorkerStatusAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetWorkerStatus");
                return await _http.GetAsync<OTWorkerStatusDto>("api/OTSync/worker-status", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetWorkerStatus lỗi");
                return ApiResponse<OTWorkerStatusDto>.Fail("Không thể kết nối server.");
            }
        }

        public async Task<ApiResponse<OTSyncResultDto>> TestWorkerRunAsync(
            DateTime date,
            CancellationToken ct = default)
        {
            try
            {
                var url = $"api/OTSync/worker-test-run?date={date:yyyy-MM-dd}";
                _logger.LogInfoIf(Debug, "[OT_CLIENT] TestWorkerRun → {Url}", url);
                return await _http.PostAsync<OTSyncResultDto>(url, new { }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] TestWorkerRun lỗi");
                return ApiResponse<OTSyncResultDto>.Fail("Không thể kết nối server.");
            }
        }

        public async Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_SYNC_CLIENT] GetDepartments");
                return await _http.GetAsync<List<DeptOption>>("api/otsync/departments", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_SYNC_CLIENT] GetDepartments error");
                return ApiResponse<List<DeptOption>>.Fail("Không thể tải danh sách phòng ban");
            }
        }

        private static string BuildUrl(string baseUrl, DateTime date, string? deptCode)
        {
            var url = $"{baseUrl}?date={date:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(deptCode))
                url += $"&deptCode={Uri.EscapeDataString(deptCode)}";
            return url;
        }
    }
}
