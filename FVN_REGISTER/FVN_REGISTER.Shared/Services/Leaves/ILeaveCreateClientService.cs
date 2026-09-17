using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Requests.Leaves;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface ILeaveCreateClientService
    {
        Task<ApiResponse<SystemMasterDataDto>> GetCombinedDataAsync(
            string empCode,
            string deptCode,
            string cvCode,
            int year,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CreateLeaveAsync(
            LeaveRequestUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CancelLeaveAsync(
            int leaveId,
            string reason,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CancelDetailAsync(
            int detailId,
            string reason,
            CancellationToken ct = default);
    }
}
