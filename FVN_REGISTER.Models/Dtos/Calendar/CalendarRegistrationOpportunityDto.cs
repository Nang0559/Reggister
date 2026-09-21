namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarRegistrationOpportunityDto
{
    public DateOnly WorkDate { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string? DisabledReason { get; init; }
}
