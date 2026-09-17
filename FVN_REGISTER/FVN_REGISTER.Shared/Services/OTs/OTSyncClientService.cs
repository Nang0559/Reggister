using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Application.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.OTs
{
   
        public class OTSyncClientService : IOTSyncClientService
        {
            private readonly IHttpClientWithAuth _http;
            private readonly ILogger<OTSyncClientService> _logger;

            public OTSyncClientService(
                IHttpClientWithAuth http,
                ILogger<OTSyncClientService> logger)
            {
                _http = http;
                _logger = logger;
            }

            public async Task<ApiResponse<OTSyncResponseDto>> SyncAsync(
                DateTime date,
                string? deptCode = null,
                CancellationToken ct = default)
            {
                try
                {
                    var url = BuildUrl("api/OTSync/sync", date, deptCode);

                    _logger.LogDebug(
                        "[OT_CLIENT] Sync → {Url}",
                        url);

                    return await _http.PostAsync<OTSyncResponseDto>(
                        url,
                        new { },
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OT_CLIENT] SyncAsync lỗi");

                    return ApiResponse<OTSyncResponseDto>.Fail(
                        "Không thể kết nối server.");
                }
            }

            public async Task<ApiResponse<OTWorkerStatusDto>> GetWorkerStatusAsync(
                CancellationToken ct = default)
            {
                try
                {
                    _logger.LogDebug(
                        "[OT_CLIENT] GetWorkerStatus");

                    return await _http.GetAsync<OTWorkerStatusDto>(
                        "api/OTSync/worker-status",
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[OT_CLIENT] GetWorkerStatus lỗi");

                    return ApiResponse<OTWorkerStatusDto>.Fail(
                        "Không thể kết nối server.");
                }
            }

            public async Task<ApiResponse<OTReconciliationResultDto>> TriggerWorkerAsync(
                string? deptCode = null,
                CancellationToken ct = default)
            {
                try
                {
                    var url = "api/OTSync/worker-trigger";

                    if (!string.IsNullOrWhiteSpace(deptCode))
                    {
                        url += $"?deptCode={Uri.EscapeDataString(deptCode)}";
                    }

                    _logger.LogDebug(
                        "[OT_CLIENT] TriggerWorker → {Url}",
                        url);

                    return await _http.PostAsync<OTReconciliationResultDto>(
                        url,
                        new { },
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[OT_CLIENT] TriggerWorkerAsync lỗi");

                    return ApiResponse<OTReconciliationResultDto>.Fail(
                        "Không thể chạy đối soát OT.");
                }
            }

            public async Task<ApiResponse<OTWorkerTestRunResponseDto>> TestWorkerRunAsync(
                DateTime date,
                CancellationToken ct = default)
            {
                try
                {
                    var url =
                        $"api/OTSync/worker-test-run?date={date:yyyy-MM-dd}";

                    _logger.LogDebug(
                        "[OT_CLIENT] TestWorkerRun → {Url}",
                        url);

                    return await _http.PostAsync<OTWorkerTestRunResponseDto>(
                        url,
                        new { },
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[OT_CLIENT] TestWorkerRun lỗi");

                    return ApiResponse<OTWorkerTestRunResponseDto>.Fail(
                        "Không thể chạy test OT.");
                }
            }

            public async Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
                CancellationToken ct = default)
            {
                try
                {
                    _logger.LogDebug(
                        "[OT_SYNC_CLIENT] GetDepartments");

                    return await _http.GetAsync<List<DeptOption>>(
                        "api/OTSync/departments",
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[OT_SYNC_CLIENT] GetDepartments error");

                    return ApiResponse<List<DeptOption>>.Fail(
                        "Không thể tải danh sách phòng ban");
                }
            }

            private static string BuildUrl(
                string baseUrl,
                DateTime date,
                string? deptCode)
            {
                var url = $"{baseUrl}?date={date:yyyy-MM-dd}";

                if (!string.IsNullOrWhiteSpace(deptCode))
                {
                    url +=
                        $"&deptCode={Uri.EscapeDataString(deptCode)}";
                }

                return url;
            }
        }
    
}
