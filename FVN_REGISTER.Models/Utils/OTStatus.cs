

namespace FVN_REGISTER.Contract.Utils
{


    // Trạng thái đơn OT — khớp với RequestStatus trong DB
    public static class OTStatus
    {
        public const string Pending = "Pending";
        public const string ApprovedLv3 = "ApprovedLv3";  // Sub-leader / Leader đã duyệt
        public const string ApprovedLv5 = "ApprovedLv5";  // Ast. Chief / Chief đã duyệt
        public const string ApprovedLv6 = "ApprovedLv6";  // A.MG / MG đã duyệt
        public const string Approved = "Approved";     // GM duyệt — hoàn tất
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";

        // Các trạng thái còn trong luồng xử lý
        public static readonly string[] ActiveStatuses =
        [
            Pending, ApprovedLv3, ApprovedLv5, ApprovedLv6
        ];

        public static string GetDisplayName(string status) => status switch
        {
            Pending => "Chờ duyệt",
            ApprovedLv3 => "Lv3 đã duyệt",
            ApprovedLv5 => "Lv5 đã duyệt",
            ApprovedLv6 => "Lv6 đã duyệt",
            Approved => "Đã duyệt hoàn tất",
            Rejected => "Từ chối",
            Cancelled => "Đã hủy",
            _ => "Không xác định"
        };

        public static string GetColor(string status) => status switch
        {
            Pending => "#FF9800",   // Orange
            ApprovedLv3 => "#2196F3",   // Blue
            ApprovedLv5 => "#00BCD4",   // Cyan
            ApprovedLv6 => "#9C27B0",   // Purple
            Approved => "#4CAF50",   // Green
            Rejected => "#F44336",   // Red
            Cancelled => "#9E9E9E",   // Grey
            _ => "#9E9E9E"
        };
    }


    // Loại OT — khớp với F03OTRequest.OTTypeCode
    
}
