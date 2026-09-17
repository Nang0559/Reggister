using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Core.Utils;


namespace FVN_REGISTER.Application.Interfaces.HrmSync
{
    /// <summary>
    /// Đọc + xử lý hàng đợi F03SyncReviewFlag cho Admin UI. Thuần Query + 1 hành động
    /// resolve đơn giản (không phải approval workflow, chỉ đánh dấu "đã xem/đã xử lý").
    /// </summary>
    public interface IHrmSyncReviewQueryService
    {
        Task<ServiceResult<List<SyncReviewFlagDto>>> GetUnresolvedAsync(
            string? entityType = null, CancellationToken ct = default);

        Task<ServiceResult<int>> CountUnresolvedAsync(CancellationToken ct = default);

        Task<ServiceResult> ResolveAsync(
            int flagId, string resolvedBy, CancellationToken ct = default);

        /// <summary>Resolve hàng loạt — dùng khi Admin đã xử lý xong F03Approver cho 1 EntityKey, đóng hết flag liên quan.</summary>
        Task<ServiceResult> ResolveByEntityAsync(
            string entityType, string entityKey, string resolvedBy, CancellationToken ct = default);
    }
}
