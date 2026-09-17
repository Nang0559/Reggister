using FVN_REGISTER.Core.Enums;
using System.Drawing;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class EmailStatusExtensions
    {
        public static string ToDisplayName(this EmailStatus status) => status switch
        {
            EmailStatus.Pending => "Đang chờ gửi",
            EmailStatus.Processing => "Đang xử lý",
            EmailStatus.Sent => "Đã gửi",
            EmailStatus.Failed => "Gửi thất bại",
            _ => "Không xác định"
        };
    }
}
