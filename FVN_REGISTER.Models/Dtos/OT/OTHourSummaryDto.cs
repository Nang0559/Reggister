using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTHourSummaryDto
    {
        public string EmployeeCode { get; set; } = "";
        public int WorkYear { get; set; }
        public int WorkMonth { get; set; }

        // Số giờ đã dùng
        public decimal UsedThisMonth { get; set; }   // Giờ OT tháng hiện tại (giới hạn 40h)
        public decimal UsedThisYear { get; set; }   // Giờ OT cả năm (giới hạn 300h)

        // Còn lại
        public decimal RemainingMonth => Math.Max(0, 40m - UsedThisMonth);
        public decimal RemainingYear => Math.Max(0, 300m - UsedThisYear);

        // Cảnh báo UI
        public bool IsMonthWarning => UsedThisMonth >= 36;   // ≥ 90% → cảnh báo
        public bool IsYearWarning => UsedThisYear >= 270;  // ≥ 90% → cảnh báo
        public bool IsMonthExceeded => UsedThisMonth > 40;
        public bool IsYearExceeded => UsedThisYear > 300;
    }
}
