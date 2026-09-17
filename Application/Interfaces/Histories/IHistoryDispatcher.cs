


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Histories
{
    public interface IHistoryDispatcher
    {
        Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
        HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
        string kind, int id, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
       string kind, int year, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult> CancelAsync(
        string kind, int id, string reason, UserIdentityDto user, CancellationToken ct);
    }
}
