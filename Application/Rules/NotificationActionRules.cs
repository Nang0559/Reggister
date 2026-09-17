using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Rules
{
    /// <summary>
    /// Quy tắc ánh xạ trạng thái duyệt (ApprovalStatus) sang hành động thông báo (NotificationAction).
    /// Dùng chung cho mọi domain (Leave, OT...) khi cần gửi in-app/email notification.
    /// </summary>
    public static class NotificationActionRules
    {
        public static NotificationAction FromApprovalStatus(string status) => status switch
        {
            "Approved" => NotificationAction.Approved,
            "Rejected" => NotificationAction.Rejected,
            _ => NotificationAction.Pending
        };
    }
}