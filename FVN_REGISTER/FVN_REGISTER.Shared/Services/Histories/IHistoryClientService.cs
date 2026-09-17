using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Shared.Services.Histories
{
    public interface IHistoryClientService
    {
        Task<ApiResponse<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, CancellationToken ct = default);
        Task<ApiResponse<ApprovalItemDetailDto>> GetDetailAsync(
            RequestKind kind, int id, CancellationToken ct = default);
        Task<ApiResponse<BalanceSummaryDto>> GetBalanceAsync(
            RequestKind kind, int year, CancellationToken ct = default);
        Task<ApiResponse<object>> CancelAsync(
            RequestKind kind, int id, string reason, CancellationToken ct = default);
    }
}
