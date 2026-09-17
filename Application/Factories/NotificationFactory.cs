using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;

namespace FVN_REGISTER.Application.Factories
{
    /// <summary>
    /// Builds notification contracts from application approval events.
    /// No transport, persistence or UI concerns belong here.
    /// </summary>
    public sealed class NotificationFactory : INotificationFactory
    {
        public CreateNotificationDto CreateApprovalNotification(
            int userId,
            string? employeeCode,
            RequestModule module,
            int requestId,
            NotificationAction action,
            int? level = null)
        {
            var moduleName = module.ToDisplayName();

            return new CreateNotificationDto
            {
                UserId = userId,
                EmployeeCode = employeeCode,
                Module = module,
                RelatedRequestId = requestId,
                ApprovalLevel = level,
                Action = action,
                Title = GetActionTitle(action, moduleName),
                Body = GetActionBody(action, moduleName, level),
                ActionUrl = $"{module.ToDetailPath()}/{requestId}",
                IsHighPriority = action == NotificationAction.Escalated
            };
        }

        private static string GetActionTitle(NotificationAction action, string moduleName) => action switch
        {
            NotificationAction.Pending => $"Đơn {moduleName} mới cần duyệt",
            NotificationAction.PendingNextLevel => $"Đơn {moduleName} chờ cấp duyệt tiếp theo",
            NotificationAction.Approved => $"Đơn {moduleName} đã được phê duyệt",
            NotificationAction.Rejected => $"Đơn {moduleName} đã bị từ chối",
            NotificationAction.Reminder => $"Nhắc nhở: Đơn {moduleName}",
            NotificationAction.Escalated => $"Cảnh báo: Đơn {moduleName} quá hạn",
            _ => "Thông báo từ hệ thống"
        };

        private static string GetActionBody(NotificationAction action, string moduleName, int? level) => action switch
        {
            NotificationAction.Pending =>
                $"Bạn có đơn {moduleName} mới từ nhân viên cần xem xét.",
            NotificationAction.PendingNextLevel =>
                $"Đơn {moduleName} đã được cấp trước phê duyệt, hiện đang chờ bạn (Cấp {level}) duyệt tiếp.",
            NotificationAction.Approved =>
                $"Chúc mừng, đơn {moduleName} của bạn đã được phê duyệt hoàn tất.",
            NotificationAction.Rejected =>
                $"Đơn {moduleName} của bạn đã bị từ chối. Vui lòng kiểm tra lại ý kiến từ người duyệt.",
            NotificationAction.Reminder =>
                $"Đây là thông báo nhắc nhở về đơn {moduleName} của bạn đang ở trạng thái chờ duyệt.",
            NotificationAction.Escalated =>
                $"Đơn {moduleName} đã vượt quá thời gian xử lý cho phép tại cấp {level}. Vui lòng kiểm tra ngay.",
            _ => $"Cập nhật trạng thái cho đơn {moduleName} của bạn."
        };
    }
}
