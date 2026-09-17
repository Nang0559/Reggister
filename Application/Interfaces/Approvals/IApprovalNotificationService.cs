using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    /// <summary>
    /// Notification dùng chung cho mọi domain duyệt (Leave, OT, ...).
    /// Nhận dữ liệu thô (id, tên, email) — không phụ thuộc entity cụ thể của domain,
    /// để Engine/Service nào cũng gọi được mà không cần biết F03leaveDay hay F03OTRequest.
    /// </summary>
    public interface IApprovalNotificationService
    {
        Task NotifyNewRequestAsync(string approverCode, string approverEmail, string approverName,
            int requestId, RequestModule requestType, string creatorName, int level,
            CancellationToken ct = default);

        Task NotifyApproverInAppAsync(string approverEmployeeCode, int requestId,
            RequestModule requestType, string creatorName, int level,
            CancellationToken ct = default);

        Task NotifyCreatorInAppAsync(int creatorUserId, string creatorEmployeeCode, string status,
            int requestId, RequestModule requestType,
            CancellationToken ct = default);

        Task NotifyEscalatedInAppAsync(int oldApproverUserId, int newApproverUserId,
            string newApproverEmployeeCode, int requestId, RequestModule requestType,
            CancellationToken ct = default);

        // ❌ SendPendingRemindersAsync — đã xóa khỏi interface
    }
}
