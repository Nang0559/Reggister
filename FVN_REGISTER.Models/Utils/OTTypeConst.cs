using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class OTTypeConst
    {
        public const string Weekday = "WEEKDAY";
        public const string Weekend = "WEEKEND";
        public const string Holiday = "HOLIDAY";

        public static string GetDisplayName(string type) => type switch
        {
            Weekday => "Ngày thường",
            Weekend => "Cuối tuần",
            Holiday => "Ngày lễ",
            _ => type
        };
    }
}
