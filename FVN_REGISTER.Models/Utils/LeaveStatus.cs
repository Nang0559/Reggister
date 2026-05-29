using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class LeaveStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Cancel = "cacel";
        public const string Rejected = "Rejected";
        public const string ApprovedLv1 = "ApprovedLv1";
        public const string ApprovedLv2 = "ApprovedLv2";

        // Hỗ trợ kiểm tra danh sách trạng thái đang xử lý
        public static readonly string[] ActiveStatuses = [Pending, ApprovedLv1, ApprovedLv2];

        public static string GetDisplayName(string status) => status switch
        {
            Pending => "Chờ duyệt",
            Approved => "Đã duyệt hoàn tất",
            Rejected => "Từ chối",
            Cancel => "Đã hủy",
            ApprovedLv1 => "Cấp 1 đã duyệt",
            ApprovedLv2 => "Cấp 2 đã duyệt",
            _ => "Không xác định"
        };
        public static string GetColor(string status) => status switch
        {
            Pending => "#FF9800",      // Orange
            Approved => "#4CAF50",     // Green
            Rejected => "#F44336",     // Red
            ApprovedLv1 => "#2196F3",  // Blue
            ApprovedLv2 => "#00BCD4",  // Cyan
            _ => "#9E9E9E"             // Grey
        };
    }
}
