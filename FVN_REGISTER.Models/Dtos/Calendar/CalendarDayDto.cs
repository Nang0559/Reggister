namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarDayDto
{
    public DateOnly Date { get; init; }
    public int? WorkYear { get; init; }
    public bool IsToday { get; init; }
    public bool IsFuture { get; init; }
    public bool IsPast { get; init; }
    public bool IsWeekend { get; init; }
    public bool IsWorkingDay { get; init; }

    public CalendarHolidayInfoDto? Holiday { get; init; }
    public CalendarShiftInfoDto? Shift { get; init; }
    public CalendarAttendanceInfoDto? Attendance { get; init; }

    public IReadOnlyList<CalendarRegistrationDto> Registrations { get; init; } = Array.Empty<CalendarRegistrationDto>();
    public IReadOnlyList<CalendarIssueDto> Issues { get; init; } = Array.Empty<CalendarIssueDto>();

    public bool CanRegisterLeave { get; init; }
    public bool CanRegisterOT { get; init; }
    public bool CanRegisterTrip { get; init; }
    public string? AvailabilityNote { get; init; }
}