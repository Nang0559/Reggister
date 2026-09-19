namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class WorkCalendarDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<WorkCalendarDayDto> Days { get; set; } = new();
    public List<WorkCalendarEventDto> Events { get; set; } = new();
}