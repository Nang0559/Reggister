using FVN_REGISTER.Core.Constants;


namespace FVN_REGISTER.Shared.Utils
{
    public static class LeaveTypeUIExtensions
    {
        // Trả về UIStyle cho trạng thái hoạt động (Dùng cho MudChip)
        public static UIStyle GetStatusUIStyle(this bool isActive) => isActive
            ? new UIStyle { Color = "success", Icon = IconConstants.Status.Approved, CssClass = "active" } // "Đang dùng"
            : new UIStyle { Color = "error", Icon = IconConstants.Status.Rejected, CssClass = "inactive" }; // "Tạm dừng"
    }
}
