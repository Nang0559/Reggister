using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Contract.Interfaces.Repositores;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface ILeaveTypeClientService
    {
        Task<ApiResponse<List<LeaveTypeDto>>> GetListAsync(
            bool? isCountedAsLeave = null,
            bool? isActive = null,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CreateAsync(
            LeaveTypeUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> UpdateAsync(
            int id,
            LeaveTypeUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default);

        Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default);
    }
}
