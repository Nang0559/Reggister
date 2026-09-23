using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Logging;
using Microsoft.Extensions.Logging;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class SharedWorkCalendarService : ISharedWorkCalendarService
{
    private readonly FVNWEBAPPContext _db;
    private readonly ICalendarModuleRegistry _registry;
    private readonly IWorkCalendarService _workCalendar;
    private readonly IAuthorizationService _authorization;
    private readonly ILogger<SharedWorkCalendarService> _logger;

    public SharedWorkCalendarService(
        FVNWEBAPPContext db,
        ICalendarModuleRegistry registry,
        IWorkCalendarService workCalendar,
        IAuthorizationService authorization,
        ILogger<SharedWorkCalendarService> logger)
    {
        _db = db;
        _registry = registry;
        _workCalendar = workCalendar;
        _authorization = authorization;
        _logger = logger;
    }

    public async Task<CalendarMonthDto> GetMonthAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        IReadOnlySet<string>? allowedModules = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (from > to)
            throw new ArgumentException("Calendar period is invalid.", nameof(from));

        var actor = await _db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserIdentityDto
            {
                UserId = x.Id,
                Permission = x.PermissionCode,
                EmployeeCode = x.EmployeeCode,
                FullName = x.FullName,
                DeptCode = x.DeptCode,
                PositionCode = x.Cvcode
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Không xác định được tài khoản hiện tại.");

        var normalizedEmployeeCode = employeeCode.Trim();
        if (normalizedEmployeeCode.Length == 0)
            throw new ArgumentException("Mã nhân viên là bắt buộc.", nameof(employeeCode));

        var employeeId = await _db.Employees
            .AsNoTracking()
            .Where(x => x.IsActive != false
                && x.EmployeeCode != null
                && x.EmployeeCode.Trim() == normalizedEmployeeCode)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhân viên có mã '{normalizedEmployeeCode}'.");

        if (!string.Equals(actor.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase)
            && !await _authorization.CanAccessAsync(
                actor,
                SecurityFunctionCodes.CalendarView,
                employeeCode,
                null,
                cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có Calendar.View hoặc ManagedScope tới nhân viên này.");
        }

        var context = new CalendarContext(employeeId, userId, from, to, allowedModules);

        var policies = await _db.CalendarModulePolicies
            .AsNoTracking()
            .Where(x => x.IsActive != false && x.IsEnabled)
            .ToDictionaryAsync(x => x.ModuleCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        // All providers are scoped to the same FVNWEBAPPContext. EF Core DbContext
        // is not thread-safe, so providers must execute sequentially unless each
        // provider is moved to an independent DbContextFactory scope.
        var results = new List<IReadOnlyList<CalendarItemDto>>();
        foreach (var provider in _registry.Providers
            .Where(x => policies.ContainsKey(x.ModuleCode)
                && (allowedModules is null || allowedModules.Contains(x.ModuleCode))))
        {
            results.Add(await provider.GetItemsAsync(context, cancellationToken));
        }

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

        // Existing data, including attendance/mismatch, takes precedence over registration.
        var occupiedDates = items
            .Where(x => x.ModuleCode != "ATTENDANCE"
                || (x.StatusCode != "NO_ATTENDANCE" && x.StatusCode != "HOLIDAY"))
            .Select(x => x.WorkDate)
            .ToHashSet();

        var opportunities = await _workCalendar.GetRegistrationOpportunitiesAsync(
            normalizedEmployeeCode,
            from.ToDateTime(TimeOnly.MinValue),
            to.ToDateTime(TimeOnly.MinValue),
            cancellationToken);

        opportunities = opportunities
            .Where(x => !occupiedDates.Contains(x.WorkDate)
                && (allowedModules is null || allowedModules.Contains(x.ModuleCode)))
            .ToArray();

        return new CalendarMonthDto
            {
                From = from,
                To = to,
                Items = items,
                Alerts = alerts,
                RegistrationOpportunities = opportunities
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogErrorIf(
                enabled: true,
                exception: ex,
                message: "SharedWorkCalendarService.GetMonthAsync failed. EmployeeCode={EmployeeCode}, UserId={UserId}, From={From}, To={To}",
                employeeCode,
                userId,
                from,
                to);

            throw;
        }
    }

    public async Task<IReadOnlyList<CalendarAlertItemDto>> GetAlertsAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        IReadOnlySet<string>? allowedModules = null,
        CancellationToken cancellationToken = default)
    {
        var result = await GetMonthAsync(employeeCode, userId, from, to, allowedModules, cancellationToken);
        return result.Alerts;
    }
}
