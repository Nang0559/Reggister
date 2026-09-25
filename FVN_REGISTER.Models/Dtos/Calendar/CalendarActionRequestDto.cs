
namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarActionRequestDto
{
    public DateOnly Date { get; init; }
    public string IssueCode { get; init; } = string.Empty;
    public string ActionCode { get; init; } = string.Empty;
    public int? RequestId { get; init; }
    public long? ReconciliationId { get; init; }
    public string? DetailRoute { get; init; }
    public string? Reason { get; init; }
}
