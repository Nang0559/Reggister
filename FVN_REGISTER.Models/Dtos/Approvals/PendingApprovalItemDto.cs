using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;



namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class PendingApprovalItemDto
    {
        // 1. Đổi tên cho đồng bộ với interface
        public int RequestId { get; set; }

        public RequestModule Kind { get; set; }

        // ── Hiển thị UI ──
        public string KindDisplay => Kind.ToDisplayName();
        public string UnitDisplay => Kind.ToUnitLabel();
    

        // ── Thông tin định danh ──
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;

        // ── Thời gian & Đơn vị (Dùng Nullable để tránh lỗi dữ liệu) ──
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal TotalUnits { get; set; }
        public DateTime? SubmittedAt { get; set; }   
        public int? TimeoutDays { get; set; }

        // ── Workflow & Trạng thái ──
        public List<ApprovalStepCalculatedDto> ApprovalSteps { get; set; } = new();

        // Lấy thông tin cấp duyệt hiện tại từ danh sách steps đã tính toán
        public int CurrentApprovalLevel => ApprovalSteps
            .OrderBy(s => s.Level)
            .FirstOrDefault(s => s.Status == DecisionType.Pending)?.Level ?? 0;

        public string? CurrentApproverName => ApprovalSteps
            .FirstOrDefault(s => s.Level == CurrentApprovalLevel)?.ApproverName;

        // Kiểm tra xem đơn này có đang ở trạng thái bị "Override" bởi Admin hay không
        public bool IsCurrentlyOverridden => ApprovalSteps.Any(s => s.IsOverridden);

        // Cờ này được gán từ Engine dựa trên quyền của người đang đăng nhập
        public bool CanApprove { get; set; }

        // Thêm Summary để UI hiển thị nhanh (VD: "Nghỉ phép 2 ngày")
        public string Summary => $"{TotalUnits} {UnitDisplay}";
    }

}
