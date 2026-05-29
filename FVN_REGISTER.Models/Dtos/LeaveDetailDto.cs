

namespace FVN_REGISTER.Contract.Dtos
{
    public record LeaveDetailDto(
    DateTime LeaveDate,
    string LeaveTypeCode,
    int TinhPhep,
    bool IsHalfDay,
    string? HalfDayOption,
    decimal DayValue
);
}
