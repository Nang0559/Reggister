using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public class OTSyncClientService : IOTSyncClientService
    {
        private readonly IHttpClientWithAuth _http;

        public OTSyncClientService(IHttpClientWithAuth http)
        {
            _http = http;
        }

        public async Task<ApiResponse<OTSyncResponseDto>> SyncAsync(
            DateTime date,
            string? deptCode = null,
            CancellationToken ct = default)
        {
            try
            {
                var url = BuildUrl("api/OTSync/sync", date, deptCode);
                return await _http.PostAsync<OTSyncResponseDto>(url, new { }, ct);
            }
            catch
            {
                return ApiResponse<OTSyncResponseDto>.Fail("Không thể kết nối server.");
            }
        }

        public Task<ApiResponse<OTWorkerStatusDto>> GetWorkerStatusAsync(
            CancellationToken ct = default)
            => _http.GetAsync<OTWorkerStatusDto>("api/OTSync/worker-status", ct);

        public async Task<ApiResponse<OTReconciliationResultDto>> TriggerWorkerAsync(
            string? deptCode = null,
            CancellationToken ct = default)
        {
            try
            {
                var url = "api/OTSync/worker-trigger";
                if (!string.IsNullOrWhiteSpace(deptCode))
                    url += $"?deptCode={Uri.EscapeDataString(deptCode)}";

                return await _http.PostAsync<OTReconciliationResultDto>(url, new { }, ct);
            }
            catch
            {
                return ApiResponse<OTReconciliationResultDto>.Fail("Không thể chạy đối chiếu OT.");
            }
        }

        public async Task<ApiResponse<OTWorkerTestRunResponseDto>> TestWorkerRunAsync(
            DateTime date,
            CancellationToken ct = default)
        {
            try
            {
                var url = $"api/OTSync/worker-test-run?date={date:yyyy-MM-dd}";
                return await _http.PostAsync<OTWorkerTestRunResponseDto>(url, new { }, ct);
            }
            catch
            {
                return ApiResponse<OTWorkerTestRunResponseDto>.Fail("Không thể chạy test OT.");
            }
        }

        public Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<DeptOption>>("api/OTSync/departments", ct);

        private static string BuildUrl(
            string baseUrl,
            DateTime date,
            string? deptCode)
        {
            var url = $"{baseUrl}?date={date:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(deptCode))
                url += $"&deptCode={Uri.EscapeDataString(deptCode)}";
            return url;
        }
    }
}
