namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarAvailabilityDto
{
    public DateTime Date { get; set; }
    public bool CanRegisterLeave { get; set; }
    public bool CanRegisterOT { get; set; }
    public bool CanRegisterTrip { get; set; }
    public List<string> Warnings { get; set; } = new();
}