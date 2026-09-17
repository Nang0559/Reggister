using FVN_REGISTER.Contract.Dtos.Employees;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class OtMonthlyUIExtensions
    {
        public static string ToMonthLabel(this OtMonthlyDto dto) => $"Tháng {dto.Month}";
    }
}
