using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Notifications
{
    public class CreateNotificationDto
    {
        // Thông tin người nhận
        public int UserId { get; set; }
        public string? EmployeeCode { get; set; }

        // Nội dung hiển thị
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }

        // Đường dẫn điều hướng (Được gán tự động bởi Factory)
        public string ActionUrl { get; set; } = string.Empty;

        // Định danh đối tượng (Dùng để build link hoặc truy vấn chi tiết)
        public RequestModule Module { get; set; }
        public int RelatedRequestId { get; set; }

        // Thông tin quy trình duyệt
        public int? ApprovalLevel { get; set; }
        public NotificationAction Action { get; set; }

        // Bổ sung: Flag nếu muốn hiển thị ngay dưới dạng Toast hoặc Popup
        public bool IsHighPriority { get; set; } = false;

        // Bổ sung: Dữ liệu mở rộng (JSON) nếu cần thêm thông tin phụ trợ (VD: Tên người duyệt)
        public string? Metadata { get; set; }
    }
}
