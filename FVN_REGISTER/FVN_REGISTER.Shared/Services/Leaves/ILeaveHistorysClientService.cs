

using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface ILeaveHistorysClientService
    {
        Task<ApiResponse<List<LeaveRequestViewModel>>> GetHistoryAsync(
            int? year,
            string? status,
            CancellationToken ct = default);
    }
}
