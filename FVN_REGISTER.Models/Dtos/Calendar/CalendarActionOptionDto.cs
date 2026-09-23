namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarActionOptionDto
{
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Kind { get; init; } = "NAVIGATION";
    public string? DetailRoute { get; init; }
    public int? RequestId { get; init; }
    public bool RequiresReason { get; init; }
    public bool RequiresAttachment { get; init; }
}