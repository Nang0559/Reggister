using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Calendar;

public sealed class WorkCalendarClientService : IWorkCalendarClientService
{
    private readonly IHttpClientWithAuth _http;

    public WorkCalendarClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<WorkCalendarDto>> GetAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        _http.GetAsync<WorkCalendarDto>($"api/calendar/me?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public Task<ApiResponse<CalendarAvailabilityDto>> GetAvailabilityAsync(DateTime date, CancellationToken ct = default) =>
        _http.GetAsync<CalendarAvailabilityDto>($"api/calendar/me/availability?date={date:yyyy-MM-dd}", ct);
}