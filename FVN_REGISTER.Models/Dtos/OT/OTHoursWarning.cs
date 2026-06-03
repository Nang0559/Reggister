using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTHoursWarning
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public decimal WeeklyHours { get; set; }  // Tổng OT trong tuần
        public decimal MonthlyHours { get; set; }  // Tổng OT trong tháng
        public bool ExceedsWeekly { get; set; }  // Vượt 40h/tuần
        public bool ExceedsMonthly { get; set; }  // Vượt 300h/tháng (năm)
        public string Message { get; set; } = string.Empty;
    }
}
