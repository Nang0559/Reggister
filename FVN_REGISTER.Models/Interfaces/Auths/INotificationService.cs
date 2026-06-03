using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;


namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    /// <summary>
    /// Quản lý in-app notification (badge, danh sách, mark-read).
    /// KHÔNG liên quan đến LeaveNotificationService (chỉ gửi email).
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Tạo thông báo mới và push realtime qua SignalR</summary>
        Task CreateAsync(CreateNotificationDto dto, CancellationToken ct = default);

        /// <summary>Lấy danh sách thông báo của user (phân trang)</summary>
        Task<List<NotificationDto>> GetByUserAsync(int userId, int page = 1, int pageSize = 20, CancellationToken ct = default);

        /// <summary>Lấy số thông báo chưa đọc (dùng cho badge)</summary>
        Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);

        /// <summary>Đánh dấu đã đọc một thông báo</summary>
        Task<ServiceResult> MarkReadAsync(int notificationId, int userId, CancellationToken ct = default);

        /// <summary>Đánh dấu tất cả đã đọc</summary>
        Task<ServiceResult> MarkAllReadAsync(int userId, CancellationToken ct = default);

        /// <summary>
        /// Helper: Tạo thông báo khi có đơn nghỉ mới chờ duyệt.
        /// Gọi từ LeaveService.CreateLeaveAsync()
        /// </summary>
        //Task NotifyPendingLeaveAsync(int approverUserId, string employeeName, int leaveId, CancellationToken ct = default);

        ///// <summary>
        ///// Helper: Tạo thông báo khi đơn được duyệt/từ chối.
        ///// Gọi từ LeaveService.ApproveAsync() / RejectAsync()
        ///// </summary>
        //Task NotifyLeaveStatusChangedAsync(int requesterUserId, string status, int leaveId, CancellationToken ct = default);
        Task PushToUserAsync(int userId, AppNotification entity, CancellationToken ct);
    }
}
