using FVN_REGISTER.Shared.Constants;


namespace FVN_REGISTER.Shared.Utils.Helpers
{
    public static class BoolUIExtensions
    {
        /// <summary>Màu trạng thái Active/Inactive chuẩn (VD: IsActive, IsEnabled).</summary>
        public static string ToActiveColor(this bool isActive)
            => isActive ? UIConstants.ColorSuccess : UIConstants.ColorDefault;

        /// <summary>Nhãn hiển thị Active/Inactive chuẩn.</summary>
        public static string ToActiveLabel(this bool isActive)
            => isActive ? "Đang hoạt động" : "Ngừng hoạt động";
    }
}
