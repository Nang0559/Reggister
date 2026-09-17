using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfMonth(this DateTime date) => new(date.Year, date.Month, 1);

        public static DateTime EndOfMonth(this DateTime date) => date.StartOfMonth().AddMonths(1).AddDays(-1);

        // Kiểm tra xem ngày này có phải là cuối tuần không (để tính OT/Leave)
        public static bool IsWeekend(this DateTime date) => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}
