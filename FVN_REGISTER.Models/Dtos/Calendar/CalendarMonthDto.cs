namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarMonthDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyList<CalendarItemDto> Items { get; init; } = Array.Empty<CalendarItemDto>();
    public IReadOnlyList<CalendarAlertItemDto> Alerts { get; init; } = Array.Empty<CalendarAlertItemDto>();
    public IReadOnlyList<CalendarRegistrationOpportunityDto> RegistrationOpportunities { get; init; } = Array.Empty<CalendarRegistrationOpportunityDto>();
}
