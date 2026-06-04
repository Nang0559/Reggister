using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class OTTypeConst
    {
        public const string Weekday = "WEEKDAY";  // Ngày thường — hệ số 1.5
        public const string Weekend = "WEEKEND";  // Cuối tuần — hệ số 2.0
        public const string Holiday = "HOLIDAY";  // Ngày lễ — hệ số 3.0

        public static string GetDisplayName(string code) => code switch
        {
            Weekday => "Ngày thường",
            Weekend => "Cuối tuần",
            Holiday => "Ngày lễ",
            _ => "Không xác định"
        };

        public static decimal GetRateMultiplier(string code) => code switch
        {
            Weekend => 2.0m,
            Holiday => 3.0m,
            _ => 1.5m   // Weekday default
        };
    }
}
