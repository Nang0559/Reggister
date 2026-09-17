


using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Reports
{
    public class ReportQueryDto
    {
        public ReportType Type { get; set; }

        // Sửa lỗi khởi tạo cho DateOnly
        public DateTime? FromDate { get; set; } 
        public DateTime? ToDate { get; set; }

        public string? DeptCode { get; set; }
        public string? EmployeeCode { get; set; }
        public int? WorkYear { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? GroupBy { get; set; }
        public bool IncludeChart { get; set; } = true;
        public string ExportFormat { get; set; } = "none";
    }
}
