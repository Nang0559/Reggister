using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class ApprovalStatusUIExtensions
    {
        // 1. Dùng UIStyle để đồng bộ với Module
        public static UIStyle GetUIStyle(this ApprovalStatus status) => status switch
        {
            ApprovalStatus.Draft => new UIStyle { Icon = IconConstants.Status.Draft, Color = "default" },
            ApprovalStatus.Pending => new UIStyle { Icon = IconConstants.Status.Pending, Color = "info" },
            ApprovalStatus.InProgress => new UIStyle { Icon = IconConstants.Status.InProgress, Color = "warning" },
            ApprovalStatus.Approved => new UIStyle { Icon = IconConstants.Status.Approved, Color = "success" },
            ApprovalStatus.Rejected => new UIStyle { Icon = IconConstants.Status.Rejected, Color = "error" },
            ApprovalStatus.Cancelled => new UIStyle { Icon = IconConstants.Status.Cancelled, Color = "dark" },
            ApprovalStatus.Escalated => new UIStyle { Icon = IconConstants.Status.Escalated, Color = "warning" },
            _ => new UIStyle { Icon = IconConstants.Status.Draft, Color = "default" }
        };
    }
}
