namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarRegistrationDto
{
    public string ModuleCode { get; init; } = string.Empty;
    public int? RequestId { get; init; }
    public string? SubTypeCode { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public bool IsHalfDay { get; init; }
    public decimal DayValue { get; init; }
    public Guid? ActionId { get; init; }
    public string? DetailRoute { get; init; }
}