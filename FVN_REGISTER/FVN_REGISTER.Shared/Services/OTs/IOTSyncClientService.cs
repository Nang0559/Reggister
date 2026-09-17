using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public interface IOTSyncClientService
    {
        Task<ApiResponse<OTSyncResponseDto>> SyncAsync(
            DateTime date,
            string? deptCode = null,
            CancellationToken ct = default);

        Task<ApiResponse<OTWorkerStatusDto>> GetWorkerStatusAsync(
            CancellationToken ct = default);

        Task<ApiResponse<OTReconciliationResultDto>> TriggerWorkerAsync(
            string? deptCode = null,
            CancellationToken ct = default);

        Task<ApiResponse<OTWorkerTestRunResponseDto>> TestWorkerRunAsync(
            DateTime date,
            CancellationToken ct = default);

        Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
            CancellationToken ct = default);
    }
}
