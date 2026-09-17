using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Notifications
{
    /// <summary>
    /// Quản lý in-app notification (badge, danh sách, mark-read).
    /// KHÔNG liên quan đến LeaveNotificationService (chỉ gửi email).
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Tạo thông báo mới, lưu DB, rồi tự động push realtime qua SignalR.</summary>
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default);

        /// <summary>Lấy danh sách thông báo của user (phân trang).</summary>
        Task<List<NotificationDto>> GetByUserAsync(
            int userId, int page = 1, int pageSize = 20, CancellationToken ct = default);

        /// <summary>Lấy số thông báo chưa đọc (dùng cho badge).</summary>
        Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);

        /// <summary>Đánh dấu đã đọc một thông báo.</summary>
        Task<ServiceResult> MarkReadAsync(int notificationId, int userId, CancellationToken ct = default);

        /// <summary>Đánh dấu tất cả đã đọc.</summary>
        Task<ServiceResult> MarkAllReadAsync(int userId, CancellationToken ct = default);
    }
}
