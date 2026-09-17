

namespace FVN_REGISTER.Contract.Dtos.OTTypeDtos
{
    public static class OTTypeDisplayExtensions
    {
        public static string ToRateDisplay(this OTTypeDto dto)
            => $"x{dto.RateMultiplier:0.0}";

        public static string ToDisplayName(this string code, IEnumerable<OTTypeDto> typeList)
            => typeList.FirstOrDefault(x => x.OTTypeCode == code)?.OTTypeName ?? code;

        public static IEnumerable<OTTypeDto> GetActiveTypes(this IEnumerable<OTTypeDto> typeList)
            => typeList.Where(x => x.IsActive).OrderBy(x => x.OTTypeCode);
    }
}
