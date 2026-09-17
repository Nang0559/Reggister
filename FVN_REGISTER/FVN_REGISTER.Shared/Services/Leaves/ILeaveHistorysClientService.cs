using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface ILeaveHistorysClientService
    {
        Task<ApiResponse<PaginationResult<LeaveSummaryDto>>> GetHistoryAsync(
            int? year,
            string? status,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);
    }
}
