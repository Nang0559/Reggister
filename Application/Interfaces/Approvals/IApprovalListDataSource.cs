
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;


namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    /// <summary>
    /// Domain-specific adapter: biết cách lấy RequestType và load dữ liệu hiển thị
    /// theo danh sách RequestId, dùng chung cho ApprovalListService.
    /// </summary>
    public interface IApprovalListDataSource<TRow> where TRow : IPendingRequestRow, IHasAttachments
    {
        RequestModule RequestType { get; } // hoặc kiểu string tuỳ bạn đang dùng, giữ nguyên như cũ

        Task<List<TRow>> GetActiveRequestsByIdsAsync(List<int> requestIds, CancellationToken ct);

        /// <summary>Domain cụ thể (Leave/OT) tự biết cách map Row của mình sang DTO chung hiển thị Dashboard.</summary>
        PendingApprovalItemDto ToPendingItem(TRow row, bool canApprove);
    }
}
