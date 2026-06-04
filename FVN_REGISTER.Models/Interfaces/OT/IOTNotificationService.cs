using Azure.Core;
using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTNotificationService
    {
        // Gửi email yêu cầu phê duyệt tới approver cấp tiếp theo
        Task SendApprovalRequestAsync(
            string approverEmail,
            string approverName,
            F03OTRequest otRequest,
            string creatorName,
            int level,
            CancellationToken ct = default);

        // Gửi thông báo kết quả (Approved / Rejected) cho người tạo đơn
        Task SendStatusChangedAsync(
            string targetEmail,
            string targetName,
            string status,
            F03OTRequest otRequest,
            CancellationToken ct = default);

        // Quét và gửi nhắc nhở các đơn quá hạn chưa được duyệt
        Task SendPendingRemindersAsync(CancellationToken ct = default);
    }
}
