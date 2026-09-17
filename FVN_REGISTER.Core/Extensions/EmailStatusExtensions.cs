using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Core.Extensions
{
    public static class EmailStatusExtensions
    {
        public static string ToDisplayName(this EmailStatus status) => status switch
        {
            EmailStatus.Pending => "Đang chờ gửi",
            EmailStatus.Processing => "Đang xử lý",
            EmailStatus.Retry => "Đang thử lại",
            EmailStatus.Sent => "Đã gửi",
            EmailStatus.Failed => "Gửi thất bại",
            EmailStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }
}
