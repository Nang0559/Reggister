
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Shared.Services.Histories
{
    
        public interface IHistoryClientService
        {
            Task<ApiResponse<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
                HistoryFilterDto filter,
                CancellationToken ct = default);

            Task<ApiResponse<HistoryItemDetailDto>> GetDetailAsync(
                RequestModule kind,
                int id,
                CancellationToken ct = default);

            Task<ApiResponse<BalanceSummaryDto>> GetBalanceAsync(
                RequestModule kind,
                int year,
                CancellationToken ct = default);

            Task<ApiResponse<object>> CancelAsync(
                RequestModule kind,
                int id,
                string reason,
                CancellationToken ct = default);
        }
    
}
