

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Reports
{
    public class ReportResultDto
    {
        public ReportType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public int TotalRows { get; set; }

        // Dữ liệu bảng (mỗi row là Dictionary để linh hoạt)
        public List<Dictionary<string, object?>> Rows { get; set; } = new();

        // Định nghĩa cột (để UI render đúng header)
        public List<ReportColumnDef> Columns { get; set; } = new();

        // Dữ liệu biểu đồ
        public ReportChartData? Chart { get; set; }

        // Tóm tắt tổng hợp
        public Dictionary<string, object?> Summary { get; set; } = new();
    }

}
