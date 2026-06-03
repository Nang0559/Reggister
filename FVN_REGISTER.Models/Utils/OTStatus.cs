using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    // OTStatus.cs
    public static class OTStatus
    {
        // Mới tạo, chờ bước 3 (SubLeader) hoặc bước 4 nếu văn phòng
        public const string Pending = "Pending";

        // Đã qua Sub.Leader, chờ BCH Công đoàn (Bước 4)
        public const string ApprovedStep3 = "ApprovedStep3";

        // Đã qua BCH CĐ, chờ Ast.Chief/Chief (Bước 5)
        public const string ApprovedStep4 = "ApprovedStep4";

        // Đã qua Chief, chờ A.MG/MG quyết định (Bước 6)
        public const string ApprovedStep5 = "ApprovedStep5";

        // Đã qua MG — nếu không cần GM thì = hoàn tất
        // Nếu cần GM thì chờ GM duyệt (Bước 7)
        public const string ApprovedStep6 = "ApprovedStep6";

        // Hoàn tất — tất cả cấp đã duyệt
        public const string Approved = "Approved";

        // Từ chối ở bất kỳ bước nào
        public const string Rejected = "Rejected";

        // Người tạo đơn hủy
        public const string Cancelled = "Cancelled";

        // Mảng trạng thái "đang xử lý" (chưa kết thúc)
        public static readonly string[] ActiveStatuses =
        {
            Pending,
            ApprovedStep3,
            ApprovedStep4,
            ApprovedStep5,
            ApprovedStep6
        };

        public static string GetDisplayName(string status) => status switch
        {
            Pending => "Chờ xác nhận (Sub.Leader/BCH CĐ)",
            ApprovedStep3 => "Sub.Leader đã ký — Chờ BCH CĐ",
            ApprovedStep4 => "BCH CĐ đã ký — Chờ Ast.Chief/Chief",
            ApprovedStep5 => "Chief đã ký — Chờ A.MG/MG",
            ApprovedStep6 => "MG đã ký — Chờ GM (nếu cần)",
            Approved => "Đã duyệt hoàn tất",
            Rejected => "Từ chối",
            Cancelled => "Đã hủy",
            _ => "Không xác định"
        };

        public static string GetColor(string status) => status switch
        {
            Pending => "#FF9800",
            ApprovedStep3 => "#64B5F6",
            ApprovedStep4 => "#4FC3F7",
            ApprovedStep5 => "#81C784",
            ApprovedStep6 => "#AED581",
            Approved => "#4CAF50",
            Rejected => "#F44336",
            Cancelled => "#9E9E9E",
            _ => "#BDBDBD"
        };
    }
}
