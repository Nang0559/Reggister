using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Histories
{
    /// <summary>
    /// Application port implemented by an Infrastructure history handler for one request kind.
    /// </summary>
    public interface IHistoryHandler
    {
        string Kind { get; }

        Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
            int id, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
            int year, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult> CancelAsync(
            int id, string reason, UserIdentityDto user, CancellationToken ct);
    }
}
