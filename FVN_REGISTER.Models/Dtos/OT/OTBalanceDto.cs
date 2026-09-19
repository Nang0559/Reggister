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
        public string EmployeeName { get; set; } = string.Empty; // 👈 thêm để hiển thị trên UI
        public int Year { get; set; }
        public int Month { get; set; }

        // Theo ngày
        public decimal UsedHoursToday { get; set; }
        public decimal DailyLimit { get; set; } = 4m;
        public decimal DailyRemainingHours => DailyLimit - UsedHoursToday;       // thêm mới
        public double DailyUsedPercent =>                                          // thêm mới
            DailyLimit == 0 ? 0 : (double)(UsedHoursToday / DailyLimit * 100);

        // Theo tuần (Thứ 2 → Chủ nhật, theo tuần lịch ISO-like của ngày hiện tại)
        public decimal UsedHoursThisWeek { get; set; }
        public decimal? WeeklyLimit { get; set; }
        public decimal? RemainingWeekly => WeeklyLimit.HasValue ? WeeklyLimit.Value - UsedHoursThisWeek : null;
        public double WeeklyUsedPercent =>
            !WeeklyLimit.HasValue || WeeklyLimit.Value == 0 ? 0 : (double)(UsedHoursThisWeek / WeeklyLimit.Value * 100);

        // Theo tháng
        public decimal UsedHoursThisMonth { get; set; }
        public decimal MonthlyLimit { get; set; } = 40m;
        public decimal RemainingMonthly => MonthlyLimit - UsedHoursThisMonth;
        public double MonthlyUsedPercent =>
        MonthlyLimit == 0 ? 0 : (double)(UsedHoursThisMonth / MonthlyLimit * 100);
       

        // Theo năm
        public decimal UsedHoursThisYear { get; set; }
        public decimal YearlyLimit { get; set; } = 200m; // 900 nếu đặc biệt
        public decimal RemainingYearly => YearlyLimit - UsedHoursThisYear;
        public double YearlyUsedPercent =>
      YearlyLimit == 0 ? 0 : (double)(UsedHoursThisYear / YearlyLimit * 100);

        // Cảnh báo
        public bool IsNearDailyLimit => DailyLimit > 0 && (DailyLimit - UsedHoursToday) <= 1;
        public bool IsNearWeeklyLimit => WeeklyLimit.HasValue && RemainingWeekly <= 4;
        public bool IsNearMonthlyLimit => MonthlyLimit > 0 && RemainingMonthly <= 8;
        public bool IsNearYearlyLimit => YearlyLimit > 0 && RemainingYearly <= 20;
        public bool ExceedsDailyLimit => DailyLimit > 0 && UsedHoursToday >= DailyLimit;   // 👈 thêm để validate bước 3
        public bool ExceedsMonthlyLimit => MonthlyLimit > 0 && UsedHoursThisMonth > MonthlyLimit;
        public bool ExceedsYearlyLimit => YearlyLimit > 0 && UsedHoursThisYear > YearlyLimit;

     
       
    }
}
