using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;
using FVN_REGISTER.Shared.Constants;

namespace FVN_REGISTER.Shared.Utils.Extensions
{
    public static class UIExtensions
    {
        // 1. Cho ApprovalStatus dạng string thô (dữ liệu cũ / fallback)
        public static UIStyle GetStatusStyle(this string status) => status switch
        {
            "Approved" => new UIStyle { CssClass = "text-success", Color = UIConstants.ColorSuccess, Icon = UIConstants.IconCheck },
            "Rejected" => new UIStyle { CssClass = "text-error", Color = UIConstants.ColorError, Icon = UIConstants.IconClose },
            _ => new UIStyle { CssClass = "text-warning", Color = UIConstants.ColorWarning, Icon = UIConstants.IconPending }
        };

        // 2. Cho ApprovalStatus (enum) — ủy quyền qua bản string
        public static UIStyle GetStatusStyle(this ApprovalStatus status)
            => status.ToString().GetStatusStyle();

        // 3. Cho EmailStatus — overload RIÊNG, không dùng chung switch với string
        public static UIStyle GetStatusStyle(this EmailStatus status) => status switch
        {
            EmailStatus.Sent => new UIStyle { CssClass = "text-success", Color = UIConstants.ColorSuccess, Icon = IconConstants.Email.Sent },
            EmailStatus.Failed => new UIStyle { CssClass = "text-error", Color = UIConstants.ColorError, Icon = IconConstants.Email.Failed },
            EmailStatus.Processing => new UIStyle { CssClass = "text-info", Color = UIConstants.ColorInfo, Icon = IconConstants.Email.Processing },
            _ => new UIStyle { CssClass = "text-warning", Color = UIConstants.ColorWarning, Icon = IconConstants.Email.Pending }
        };

        // 4. Logic hiển thị cho IValidatableRow — giữ nguyên
        public static string GetRowStyle(this IValidatableRow row) => row.Status switch
        {
            RowStatus.Info => UIConstants.CssRowInfo,
            RowStatus.Success => UIConstants.CssRowSuccess,
            RowStatus.Warning => UIConstants.CssRowWarning,
            RowStatus.Error => UIConstants.CssRowError,
            _ => UIConstants.CssRowDefault
        };

        // 5. Lấy icon cho RowStatus — giữ nguyên
        public static string GetStatusIcon(this RowStatus status) => status switch
        {
            RowStatus.Info => UIConstants.IconInfo,
            RowStatus.Success => UIConstants.IconSuccess,
            RowStatus.Warning => UIConstants.IconWarning,
            RowStatus.Error => UIConstants.IconError,
            _ => string.Empty
        };
    }
}
