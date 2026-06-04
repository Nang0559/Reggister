

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTEmployee
    {
        public int Id { get; set; }

        public int OTRequestId { get; set; }            // FK → F03OTRequest

        public string EmployeeCode { get; set; } = null!;

        public string? EmployeeName { get; set; }

        public string? DeptCode { get; set; }

        public string? DeptName { get; set; }

        public string? CvCode { get; set; }             // "0003" = công nhân

        public string? OTTypeCode { get; set; }         // có thể khác với OTRequest nếu cần

        // Giờ OT kế hoạch — mỗi nhân viên có thể khác nhau trong cùng 1 đơn
        public decimal OTHours { get; set; }

        // Giờ thực tế sau chấm công (bước GA lưu trữ — bước 9-10)
        public decimal? ActualHours { get; set; }

        // Hệ số lương: 1.5 (thường), 2.0 (cuối tuần), 3.0 (lễ)
        public decimal OTRateMultiplier { get; set; } = 1.5m;

        // Kết quả validate: "Valid" | "Warning" | "Exceeded"
        public string? ValidationStatus { get; set; }

        public string? ValidationMessage { get; set; }

        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        // ===== Navigation =====
        public virtual F03OTRequest OTRequest { get; set; } = null!;
    }
}
