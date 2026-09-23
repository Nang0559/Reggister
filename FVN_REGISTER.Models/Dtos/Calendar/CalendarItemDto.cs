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
    public string InteractionType { get; init; } = "INFO";
    public Guid? ActionId { get; init; }
    public string? DetailRoute { get; init; }
    public string SourceId { get; init; } = string.Empty;
    public string? SourceType { get; init; }
    public string? ParticipantId { get; init; }

    public int? ShiftId { get; init; }
    public string? ShiftAbbr { get; init; }
    public DateTime? CheckIn { get; init; }
    public DateTime? CheckOut { get; init; }
    public int WorkMinutes { get; init; }
    public int RequiredMinutes { get; init; }
    public int ActualOtMinutes { get; init; }
    public int RecognizedOtMinutes { get; init; }
    public decimal? ActualHours { get; init; }
    public decimal? RequiredHours { get; init; }
    public bool HasActual { get; init; }
    public bool HasActualOt { get; init; }
    public bool IsNumericVariance { get; init; }
}
