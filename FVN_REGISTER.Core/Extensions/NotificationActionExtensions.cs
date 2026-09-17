using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class NotificationActionExtensions
    {
        

        // Nếu bạn vẫn cần DisplayName riêng cho Notification
        public static string ToDisplayName(this NotificationAction action) => action switch
        {
            NotificationAction.Pending => "Chờ duyệt",
            NotificationAction.PendingNextLevel => "Chờ cấp tiếp theo",
            NotificationAction.Approved => "Đã phê duyệt",
            NotificationAction.Rejected => "Đã từ chối",
            NotificationAction.Reminder => "Nhắc nhở",
            NotificationAction.Escalated => "Quá hạn",
            _ => "Thông báo"
        };
    }
}
