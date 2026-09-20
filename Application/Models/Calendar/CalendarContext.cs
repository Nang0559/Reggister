namespace FVN_REGISTER.Application.Models.Calendar;

public sealed record CalendarContext(
    int EmployeeId,
    int UserId,
    DateOnly From,
    DateOnly To,
    IReadOnlySet<string>? AllowedModules = null,
    string? DeptCode = null,
    string? PositionCode = null);
