


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Histories
{
    // IHistoryHandler.cs
    public interface IHistoryHandler
    {
        string Kind { get; }

        Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct);
        Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(  // ✅ thêm vào interface
        int id, UserIdentityDto user, CancellationToken ct);
        Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
            int year, UserIdentityDto user, CancellationToken ct);

        Task<ServiceResult> CancelAsync(
            int id, string reason, UserIdentityDto user, CancellationToken ct);
    }


   
}
