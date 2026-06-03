

namespace FVN_REGISTER.Contract.Utils
{
    public static class OTStatus
    {
        public const string Pending = "Pending";
        public const string ApprovedLv1 = "ApprovedLv1";
        public const string ApprovedLv2 = "ApprovedLv2";
        public const string ApprovedLv3 = "ApprovedLv3";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";

        public static readonly string[] ActiveStatuses =
            [Pending, ApprovedLv1, ApprovedLv2, ApprovedLv3];

        public static string GetDisplayName(string status) => status switch
        {
            Pending => "Chờ duyệt",
            ApprovedLv1 => "Cấp 1 đã duyệt",
            ApprovedLv2 => "Cấp 2 đã duyệt",
            ApprovedLv3 => "Cấp 3 đã duyệt",
            Approved => "Đã duyệt hoàn tất",
            Rejected => "Từ chối",
            Cancelled => "Đã hủy",
            _ => "Không xác định"
        };

        public static string GetColor(string status) => status switch
        {
            Pending => "#FF9800",
            Approved => "#4CAF50",
            Rejected => "#F44336",
            Cancelled => "#9E9E9E",
            ApprovedLv1 => "#2196F3",
            ApprovedLv2 => "#00BCD4",
            ApprovedLv3 => "#673AB7",
            _ => "#9E9E9E"
        };

        public static string GetMudColor(string status) => status switch
        {
            Pending => "Warning",
            Approved => "Success",
            Rejected => "Error",
            Cancelled => "Default",
            _ => "Info"
        };
    }
}
