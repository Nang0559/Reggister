namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class WorkCalendarDayDto
{
    public DateTime Date { get; set; }
    public int? WorkYear { get; set; }
    public bool IsWeekend { get; set; }
    public bool IsWorkingDay { get; set; }
    public string? HolidayCode { get; set; }
    public string? HolidayName { get; set; }
    public bool CanRegisterLeave { get; set; }
    public bool CanRegisterOT { get; set; }
    public bool CanRegisterTrip { get; set; }
    public string? AvailabilityNote { get; set; }

    // HRM-compatible attendance result for the logged-in employee.
    public bool HasAttendance { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public decimal ActualWorkHours { get; set; }
    public decimal ActualOTHours { get; set; }

    // Approved OT for the same date.
    public decimal ApprovedOTHours { get; set; }
    public string? AttendanceStatus { get; set; }
    public string? AttendanceNote { get; set; }
}
