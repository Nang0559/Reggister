using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTBalanceDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }

        // Theo ngày
        public decimal UsedHoursToday { get; set; }
        public decimal DailyLimit { get; set; }       // 4h

        // Theo tháng
        public decimal UsedHoursThisMonth { get; set; }
        public decimal MonthlyLimit { get; set; }     // 40h
        public decimal RemainingMonthly => MonthlyLimit - UsedHoursThisMonth;

        // Theo năm
        public decimal UsedHoursThisYear { get; set; }
        public decimal YearlyLimit { get; set; }      // 200h (hoặc 900h đặc biệt)
        public decimal RemainingYearly => YearlyLimit - UsedHoursThisYear;

        // Cảnh báo
        public bool IsNearMonthlyLimit => RemainingMonthly <= 8;
        public bool IsNearYearlyLimit => RemainingYearly <= 20;
        public bool ExceedsMonthlyLimit => UsedHoursThisMonth > MonthlyLimit;
        public bool ExceedsYearlyLimit => UsedHoursThisYear > YearlyLimit;
    }
}
