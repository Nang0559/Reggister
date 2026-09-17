using FVN_REGISTER.Contract.Dtos.Leaves;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class LeaveRequestDtoUIExtensions
    {
        public static string GetDateRangeDisplay(this LeaveRequestDto dto) =>
            dto.StartDate != DateTime.MinValue
                ? $"{dto.StartDate:dd/MM/yyyy} - {dto.EndDate:dd/MM/yyyy}"
                : "Chưa chọn ngày";

        public static string GetSummary(this LeaveRequestDto dto) => $"{dto.TotalDay} ngày nghỉ";
    }
}
