namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class WorkCalendarEventDto
{
    public string Id { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public int? RequestId { get; set; }

    // FullCalendar presentation. Null means use the default theme.
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public string? BorderColor { get; set; }
    public bool AllDay { get; set; }
}
