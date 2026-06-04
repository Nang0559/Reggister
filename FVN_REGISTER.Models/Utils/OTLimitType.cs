using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{

    // Loại giới hạn giờ OT — khớp với F03OTLimitRule.LimitType
    public static class OTLimitType
    {
        public const string Daily = "Daily";    // 4h/ngày
        public const string Weekly = "Weekly";   // 40h/tuần (tham khảo)
        public const string Yearly = "Yearly";   // 200h/năm thường
        public const string Special = "Special";  // 900h/năm đặc biệt
    }
}
