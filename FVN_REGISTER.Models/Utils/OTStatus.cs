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
        // ===== Trạng thái =====
        public const string Pending = "Pending";       // Chờ Bước 3 (hoặc 4 nếu skip 3)
        public const string ApprovedLv3 = "ApprovedLv3";   // Bước 3 xong, chờ Bước 4
        public const string ApprovedLv4 = "ApprovedLv4";   // Bước 4 xong, chờ Bước 5
        public const string ApprovedLv5 = "ApprovedLv5";   // Bước 5 xong, chờ Bước 6
        public const string ApprovedLv6 = "ApprovedLv6";   // Bước 6 xong, chờ GM (nếu cần)
        public const string Approved = "Approved";      // Hoàn tất toàn bộ
        public const string Rejected = "Rejected";      // Bị từ chối (bất kỳ cấp)
        public const string Cancelled = "Cancelled";     // Nhân viên hủy

        public static readonly string[] ActiveStatuses =
            [Pending, ApprovedLv3, ApprovedLv4, ApprovedLv5, ApprovedLv6];

        public static string GetDisplayName(string status) => status switch
        {
            Pending => "Chờ duyệt",
            ApprovedLv3 => "Sub.Leader đã duyệt",
            ApprovedLv4 => "BCH CĐ đã duyệt",
            ApprovedLv5 => "Ast.Chief đã duyệt",
            ApprovedLv6 => "A.MG/MG đã duyệt",
            Approved => "Đã duyệt hoàn tất",
            Rejected => "Từ chối",
            Cancelled => "Đã hủy",
            _ => "Không xác định"
        };

        public static string GetColor(string status) => status switch
        {
            Pending => "#FF9800",
            ApprovedLv3 => "#2196F3",
            ApprovedLv4 => "#00BCD4",
            ApprovedLv5 => "#9C27B0",
            ApprovedLv6 => "#3F51B5",
            Approved => "#4CAF50",
            Rejected => "#F44336",
            Cancelled => "#9E9E9E",
            _ => "#9E9E9E"
        };
    }
}
