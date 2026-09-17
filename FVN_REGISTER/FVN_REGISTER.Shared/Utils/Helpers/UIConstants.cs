

using System.Runtime.CompilerServices;

namespace FVN_REGISTER.Shared.Constants
{
    public static class UIConstants
    {
        // 1. Nhóm màu sắc (Dùng cho thuộc tính Color của MudBlazor Components)
        public const string ColorSuccess = "success";
        public const string ColorError = "error";
        public const string ColorWarning = "warning";
        public const string ColorInfo = "info";
        public const string ColorDefault = "default";
        public const string ColorSecondary = "secondary";

        // 2. Nhóm CSS Styles (Dùng cho RowCssClass của bảng)
        public const string CssRowDefault = "bg-transparent";
        public const string CssRowInfo = "bg-blue-lighten-4";
        public const string CssRowSuccess = "bg-green-lighten-4";
        public const string CssRowWarning = "bg-amber-lighten-4";
        public const string CssRowError = "bg-red-lighten-4";

        // 3. Nhóm Icons (Đã thêm vào để đồng bộ)
        public const string IconInfo = "INFO";
        public const string IconSuccess = "SUCCESS";
        public const string IconWarning = "WARNING";
        public const string IconError = "ERROR";
        public const string IconCheck = "CHECK";
        public const string IconClose = "CLOSE";
        public const string IconPending = "PENDING";
    }
}
