using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Calendar;

public interface IWorkCalendarClientService
{
    Task<ApiResponse<WorkCalendarDto>> GetAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<ApiResponse<CalendarAvailabilityDto>> GetAvailabilityAsync(DateTime date, CancellationToken ct = default);
}