using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class SharedWorkCalendarService : ISharedWorkCalendarService
{
    private readonly FVNWEBAPPContext _db;
    private readonly ICalendarModuleRegistry _registry;

    public SharedWorkCalendarService(FVNWEBAPPContext db, ICalendarModuleRegistry registry)
    {
        _db = db;
        _registry = registry;
    }

    public async Task<CalendarMonthDto> GetMonthAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Calendar period is invalid.", nameof(from));

        var employeeId = await _db.Employees
            .AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeCode == employeeCode)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhân viên của tài khoản hiện tại.");

        var context = new CalendarContext(employeeId, userId, from, to);

        var policies = await _db.CalendarModulePolicies
            .AsNoTracking()
            .Where(x => x.IsActive != false && x.IsEnabled)
            .ToDictionaryAsync(x => x.ModuleCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var tasks = _registry.Providers
            .Where(x => policies.ContainsKey(x.ModuleCode))
            .Select(x => x.GetItemsAsync(context, cancellationToken));

        var results = await Task.WhenAll(tasks);
        var items = results
            .SelectMany(x => x)
            .OrderBy(x => x.WorkDate)
            .ThenByDescending(x => x.RequiresAction)
            .ThenByDescending(x => x.Severity)
            .ThenBy(x => policies.TryGetValue(x.ModuleCode, out var p) ? p.Priority : 100)
            .ThenBy(x => x.ModuleCode)
            .ToArray();

        var alerts = items
            .Where(x => x.RequiresAction || x.Severity > 0)
            .Select(x => new CalendarAlertItemDto
            {
                WorkDate = x.WorkDate,
                ModuleCode = x.ModuleCode,
                Severity = x.Severity switch
                {
                    >= 3 => "Critical",
                    2 => "Warning",
                    1 => "Attention",
                    _ => "Info"
                },
                Summary = x.Summary ?? x.StatusCode,
                RequiresAction = x.RequiresAction,
                ActionId = x.ActionId,
                DetailRoute = x.DetailRoute,
                SourceId = x.SourceId
            })
            .ToArray();

        return new CalendarMonthDto
        {
            From = from,
            To = to,
            Items = items,
            Alerts = alerts
        };
    }

    public async Task<IReadOnlyList<CalendarAlertItemDto>> GetAlertsAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var result = await GetMonthAsync(employeeCode, userId, from, to, cancellationToken);
        return result.Alerts;
    }
}
