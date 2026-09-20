namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarItemDto
{
    public DateOnly WorkDate { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public string StatusCode { get; init; } = string.Empty;
    public string? Marker { get; init; }
    public string? Summary { get; init; }
    public byte Severity { get; init; }
    public bool RequiresAction { get; init; }
    public Guid? ActionId { get; init; }
    public string? DetailRoute { get; init; }
    public string SourceId { get; init; } = string.Empty;
}
