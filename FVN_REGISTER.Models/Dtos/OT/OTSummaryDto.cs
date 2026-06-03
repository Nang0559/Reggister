using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTSummaryDto
    {
        public string EmployeeCode { get; set; } = null!;
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal UsedHoursMonth { get; set; }
        public decimal UsedHoursYear { get; set; }
        public decimal RemainingMonth => Math.Max(0, 40m - UsedHoursMonth);
        public decimal RemainingYear => Math.Max(0, 200m - UsedHoursYear);
        public bool IsNearMonthLimit => UsedHoursMonth >= 35m;  // Cảnh báo khi ≥ 35h/tháng
        public bool IsNearYearLimit => UsedHoursYear >= 180m; // Cảnh báo khi ≥ 180h/năm
    }
}
