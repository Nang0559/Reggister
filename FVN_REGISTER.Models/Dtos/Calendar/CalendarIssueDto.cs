namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarIssueDto
{
    public string Code { get; init; } = string.Empty;
    public string ModuleCode { get; init; } = string.Empty;
    public byte Severity { get; init; }
    public string Marker { get; init; } = "?";
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? SourceId { get; init; }
    public Guid? ActionId { get; init; }
    public string? DetailRoute { get; init; }
    public int? RequestId { get; init; }
    public IReadOnlyList<CalendarActionOptionDto> Actions { get; init; } = Array.Empty<CalendarActionOptionDto>();
}