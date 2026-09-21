using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Calendar;

public interface IWorkCalendarClientService
{
    Task<ApiResponse<CalendarMonthDto>> GetAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<CalendarAlertItemDto>>> GetAlertsAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<ApiResponse<CalendarAvailabilityDto>> GetAvailabilityAsync(DateTime date, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<CalendarRegistrationOpportunityDto>>> GetRegistrationOpportunitiesAsync(DateTime from, DateTime to, CancellationToken ct = default);
}