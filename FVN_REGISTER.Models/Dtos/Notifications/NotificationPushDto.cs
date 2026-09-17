

namespace FVN_REGISTER.Contract.Dtos.Notifications
{
    public class NotificationPushDto
    {
        public int UnreadCount { get; set; }
        public NotificationDto? Latest { get; set; }

        // Factory để tạo nhanh object khi có thông báo mới
        public static NotificationPushDto Create(NotificationDto latest, int unreadCount)
            => new() { Latest = latest, UnreadCount = unreadCount };
    }
}
