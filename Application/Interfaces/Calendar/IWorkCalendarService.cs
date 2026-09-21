using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface IWorkCalendarService
{
    Task<WorkCalendarDto> GetAsync(
        string employeeCode,
        string? deptCode,
        string? positionCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    Task<CalendarAvailabilityDto> GetAvailabilityAsync(
        string employeeCode,
        DateTime date,
        CancellationToken ct = default);

    Task<IReadOnlyList<CalendarRegistrationOpportunityDto>> GetRegistrationOpportunitiesAsync(
        string employeeCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}