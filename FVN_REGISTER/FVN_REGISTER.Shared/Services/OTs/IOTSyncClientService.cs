using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public interface IOTSyncClientService
    {
        Task<ApiResponse<OTSyncResultDto>> SyncAsync(
            DateTime date,
            string? deptCode = null,
            CancellationToken ct = default);

        Task<ApiResponse<OTUnconfirmedListResponse>> GetUnconfirmedAsync(
            DateTime date,
            string? deptCode = null,
            CancellationToken ct = default);

        // Thêm 2 method mới cho test
        Task<ApiResponse<OTWorkerStatusDto>> GetWorkerStatusAsync(CancellationToken ct = default);

        Task<ApiResponse<OTSyncResultDto>> TestWorkerRunAsync(
            DateTime date, CancellationToken ct = default);

        // Thêm vào interface hiện tại
        Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
            CancellationToken ct = default);
    }
}
