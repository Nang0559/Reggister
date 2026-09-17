namespace FVN_REGISTER.Core.Enums
{
    public enum NotificationAction
    {
        Pending,         // Đơn mới tạo
        PendingNextLevel,// Chờ cấp tiếp theo duyệt
        Approved,        // Đã duyệt xong
        Rejected,        // Đã từ chối
        Reminder,        // Nhắc nhở
        Escalated        // Cảnh báo quá hạn
    }
}
