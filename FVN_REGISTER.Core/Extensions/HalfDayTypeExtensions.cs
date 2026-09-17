using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class HalfDayTypeExtensions
    {
        public static string ToDisplayName(this HalfDayType type) => type switch
        {
            HalfDayType.None => "Cả ngày",
            HalfDayType.Morning => "Sáng",
            HalfDayType.Afternoon => "Chiều",
            _ => "Không xác định"
        };

        // Logic bổ trợ: Tính tỷ lệ ngày nghỉ (thường dùng để nhân hệ số)
        public static double ToDayFactor(this HalfDayType type) => type switch
        {
            HalfDayType.None => 1.0,  // Nghỉ cả ngày
            _ => 0.5   // Nghỉ nửa ngày
        };
    }
}
