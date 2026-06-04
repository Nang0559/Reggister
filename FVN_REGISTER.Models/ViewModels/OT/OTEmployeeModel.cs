using FVN_REGISTER.Contract.Utils;

using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTEmployeeModel
    {
        // --- Định danh ---
        public string EmployeeCode { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public string? DeptCode { get; set; }
        public string? DeptName { get; set; }
        public string? CvCode { get; set; }

        // --- Thông tin OT ---
        [Range(0.5, 12, ErrorMessage = "Giờ OT phải từ 0.5 đến 12")]
        public decimal OTHours { get; set; }

        public string? OTTypeCode { get; set; }

        public decimal? ActualHours { get; set; }
        private decimal? _otRateMultiplier;
        // Tính tự động từ OTTypeCode — không nhập tay
        public decimal OTRateMultiplier
        {
            get
            {
                // Nếu đã được gán giá trị thì lấy giá trị đó, nếu chưa thì tự tính theo OTTypeCode
                return _otRateMultiplier ?? OTTypeCode switch
                {
                    OTTypeConst.Weekend => 2.0m,
                    OTTypeConst.Holiday => 3.0m,
                    _ => 1.5m
                };
            }
            set => _otRateMultiplier = value;
        }

        // --- Validate real-time ---
        // "Valid" | "Warning" | "Exceeded" — string thay vì MudBlazor Color
        public string? ValidationStatus { get; set; }
        public string? ValidationMessage { get; set; }

        // Helper trả về string màu CSS — dùng được ở cả Razor lẫn API
        // Razor gọi: style="color: @emp.ValidationColor"
        public string ValidationColorCss => ValidationStatus switch
        {
            "Exceeded" => "var(--mud-palette-error)",
            "Warning" => "var(--mud-palette-warning)",
            "Valid" => "var(--mud-palette-success)",
            _ => "var(--mud-palette-text-secondary)"
        };

        // --- Ghi chú ---
        public string? Note { get; set; }
    }
}
