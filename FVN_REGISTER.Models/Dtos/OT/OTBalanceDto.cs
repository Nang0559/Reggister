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

        // Theo tháng
        public decimal UsedHoursThisMonth { get; set; }
        public decimal MonthlyLimit { get; set; } = 40m;
        public decimal RemainingMonthly => MonthlyLimit - UsedHoursThisMonth;

        // Theo năm
        public decimal UsedHoursThisYear { get; set; }
        public decimal UsedHours => UsedHoursThisYear; // 👈 alias cho OTCreate.razor dùng _model.Balance.UsedHours
        public decimal YearlyLimit { get; set; } = 200m; // 900 nếu đặc biệt
        public decimal RemainingYearly => YearlyLimit - UsedHoursThisYear;

        // Cảnh báo
        public bool IsNearDailyLimit => (DailyLimit - UsedHoursToday) <= 1;
        public bool IsNearMonthlyLimit => RemainingMonthly <= 8;
        public bool IsNearYearlyLimit => RemainingYearly <= 20;
        public bool ExceedsDailyLimit => UsedHoursToday >= DailyLimit;   // 👈 thêm để validate bước 3
        public bool ExceedsMonthlyLimit => UsedHoursThisMonth > MonthlyLimit;
        public bool ExceedsYearlyLimit => UsedHoursThisYear > YearlyLimit;

        // Helper tính % cho MudProgressLinear
        public double YearlyUsedPercent =>
            YearlyLimit == 0 ? 0 : (double)(UsedHoursThisYear / YearlyLimit * 100);
        public double MonthlyUsedPercent =>
            MonthlyLimit == 0 ? 0 : (double)(UsedHoursThisMonth / MonthlyLimit * 100);
    }
}
