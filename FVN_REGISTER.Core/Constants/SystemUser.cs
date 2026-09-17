

namespace FVN_REGISTER.Core.Constants
{
    /// <summary>
    /// Sentinel UserId cho các hành động do HỆ THỐNG tự thực hiện (background job,
    /// auto-escalation, sync worker...) — không có người dùng thật đứng sau.
    /// Dùng thống nhất thay vì rải "-1"/"0" rời rạc ở nhiều nơi.
    /// </summary>
    public static class SystemUser
    {
        public const int Id = -1;
        public const string Code = "SYSTEM";
        public const string DisplayName = "Hệ thống tự động";
    }
}
