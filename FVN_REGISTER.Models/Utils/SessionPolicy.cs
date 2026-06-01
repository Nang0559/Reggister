using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class SessionPolicy
    {
        public const int MaxMobileDevices = 1;
        public const int MaxWebDevices = 1;

        // Mobile nhớ 30 ngày nếu RememberMe
        // Web chỉ 1 ngày
        public static readonly TimeSpan MobileExpiry = TimeSpan.FromDays(30);
        public static readonly TimeSpan WebExpiry = TimeSpan.FromDays(1);
    }
}
