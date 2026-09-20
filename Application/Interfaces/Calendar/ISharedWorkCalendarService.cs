using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface ISharedWorkCalendarService
{
    Task<CalendarMonthDto> GetMonthAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarAlertItemDto>> GetAlertsAsync(
        int employeeId,
        int userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
