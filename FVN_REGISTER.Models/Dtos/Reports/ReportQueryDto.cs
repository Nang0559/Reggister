using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Reports
{
    public class ReportQueryDto
    {
        public ReportType Type { get; set; }
        public DateTime? FromDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime? ToDate { get; set; } = DateTime.Now;
        public string? DeptCode { get; set; }        // null = toàn công ty
        public string? EmployeeCode { get; set; }    // null = tất cả NV
        public int? WorkYear { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? GroupBy { get; set; }         // "dept" | "employee" | "month"
        public bool IncludeChart { get; set; } = true;
        public string ExportFormat { get; set; } = "none"; // "excel" | "pdf" | "none"
    }
}
