




using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApprovalActionDto
    {
        // Danh sách ID đơn cần xử lý hàng loạt
        public List<int> RequestIds { get; set; } = new();

      
        public RequestModule Kind { get; set; } 

        // Cấp độ duyệt hiện tại
        public int Level { get; set; }

        // true = Từ chối, false = Phê duyệt
        public bool IsReject { get; set; }

        // Ghi chú
        public string? Comment { get; set; }

        // Thông tin người thực hiện hành động (Dùng để log audit)
        public string ApproverCode { get; set; } = string.Empty;
        public int ApproverPermission { get; set; }

        // Kiểm tra quyền (Tuỳ chọn: Nếu bạn không muốn lấy User từ Claims)
        public string? ApproverPositionCode { get; set; }
    }
}
