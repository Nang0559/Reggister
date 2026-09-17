using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Shared.Utils
{
    public static class NotificationActionUIExtensions
    {
        public static UIStyle GetUIStyle(this NotificationAction action) => action switch
        {
            NotificationAction.Pending => new UIStyle { Icon = IconConstants.Status.Pending, Color = "info" },
            NotificationAction.PendingNextLevel => new UIStyle { Icon = IconConstants.Status.InProgress, Color = "info" },
            NotificationAction.Approved => new UIStyle { Icon = IconConstants.Status.Approved, Color = "success" },
            NotificationAction.Rejected => new UIStyle { Icon = IconConstants.Status.Rejected, Color = "error" },
            NotificationAction.Reminder => new UIStyle { Icon = IconConstants.Notification.Reminder, Color = "warning" },
            NotificationAction.Escalated => new UIStyle { Icon = IconConstants.Status.Escalated, Color = "warning" },
            _ => new UIStyle { Icon = IconConstants.Notification.System, Color = "default" }
        };

        
    }
}
