


using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    // <summary>
    /// "Hộp thư chờ duyệt" tổng hợp xuyên module (Leave + OT + ...) cho 1 approver.
    /// Pending items của mọi module đều đi qua approval workflow/engine path; không có
    /// datasource/service pending-list riêng theo module.
    ///
    /// Không có method lấy chi tiết 1 item xuyên module: muốn xem chi tiết, FE tự điều hướng
    /// theo Module (RequestModule.Leave -> ILeaveOrchestrator.GetDetailsAsync,
    /// RequestModule.Overtime -> IOTOrchestrator.GetDetailsAsync).
    /// </summary>
    public interface IApprovalInboxService
    {
        /// <summary>Toàn bộ item đang chờ approver duyệt, gộp mọi module, gom theo Level.</summary>
        Task<ServiceResult<List<PendingApprovalGroupDto>>> GetPendingAsync(
            UserIdentityDto user,
            CancellationToken ct = default);

        /// <summary>Duyệt hàng loạt item CÙNG 1 module, cùng 1 level.</summary>
        Task<ServiceResult> ApproveItemsAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string? comment,
            UserIdentityDto user,
            CancellationToken ct = default);

        /// <summary>Từ chối hàng loạt item CÙNG 1 module, cùng 1 level. Comment bắt buộc.</summary>
        Task<ServiceResult> RejectItemsAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string comment,
            UserIdentityDto user,
            CancellationToken ct = default);
    }
}
