namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarShiftInfoDto
{
    public int? ShiftId { get; init; }
    public string? ShiftAbbr { get; init; }
    public int RequiredMinutes { get; init; }
}