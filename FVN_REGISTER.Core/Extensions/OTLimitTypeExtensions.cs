using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class OTLimitTypeExtensions
    {
        public static string ToDisplayName(this OTLimitType type) => type switch
        {
            OTLimitType.Daily => "Hạn mức ngày",
            OTLimitType.Weekly => "Hạn mức tuần",
            OTLimitType.Yearly => "Hạn mức năm",
            OTLimitType.Special => "Hạn mức đặc biệt",
            _ => type.ToString()
        };

        // Trả về UIStyle để đồng bộ với các module trước
        public static UIStyle GetUIStyle(this OTLimitType type) => type switch
        {
            OTLimitType.Daily => new UIStyle { Color = "info", CssClass = "limit-daily" },
            OTLimitType.Weekly => new UIStyle { Color = "primary", CssClass = "limit-weekly" },
            OTLimitType.Yearly => new UIStyle { Color = "success", CssClass = "limit-yearly" },
            OTLimitType.Special => new UIStyle { Color = "error", CssClass = "limit-special" },
            _ => new UIStyle { Color = "default" }
        };
    }
}
