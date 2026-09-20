namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarAlertItemDto
{
    public DateOnly WorkDate { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public string Severity { get; init; } = "Info";
    public string Summary { get; init; } = string.Empty;
    public bool RequiresAction { get; init; }
    public Guid? ActionId { get; init; }
    public string? DetailRoute { get; init; }
    public string SourceId { get; init; } = string.Empty;
}
