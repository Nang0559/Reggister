using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Calendar;

public sealed class WorkCalendarClientService : IWorkCalendarClientService
{
    private readonly IHttpClientWithAuth _http;

    public WorkCalendarClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<CalendarMonthDto>> GetAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        _http.GetAsync<CalendarMonthDto>($"api/calendar/me?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public Task<ApiResponse<CalendarMonthDto>> GetForEmployeeAsync(
        string employeeCode, DateTime from, DateTime to, CancellationToken ct = default) =>
        _http.GetAsync<CalendarMonthDto>(
            $"api/calendar/employee/{Uri.EscapeDataString(employeeCode)}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public Task<ApiResponse<IReadOnlyList<CalendarAlertItemDto>>> GetAlertsForEmployeeAsync(
        string employeeCode, DateTime from, DateTime to, CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<CalendarAlertItemDto>>(
            $"api/calendar/employee/{Uri.EscapeDataString(employeeCode)}/alerts?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public Task<ApiResponse<IReadOnlyList<CalendarAlertItemDto>>> GetAlertsAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<CalendarAlertItemDto>>($"api/calendar/me/alerts?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public Task<ApiResponse<CalendarAvailabilityDto>> GetAvailabilityAsync(DateTime date, CancellationToken ct = default) =>
        _http.GetAsync<CalendarAvailabilityDto>($"api/calendar/me/availability?date={date:yyyy-MM-dd}", ct);

    public Task<ApiResponse<IReadOnlyList<CalendarRegistrationOpportunityDto>>> GetRegistrationOpportunitiesAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<CalendarRegistrationOpportunityDto>>($"api/calendar/me/registration-opportunities?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);
}