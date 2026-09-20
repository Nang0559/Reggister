using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface ISharedWorkCalendarService
{
    Task<CalendarMonthDto> GetMonthAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        IReadOnlySet<string>? allowedModules = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarAlertItemDto>> GetAlertsAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        IReadOnlySet<string>? allowedModules = null,
        CancellationToken cancellationToken = default);
}
