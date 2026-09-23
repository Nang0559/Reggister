using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class SharedWorkCalendarService : ISharedWorkCalendarService
{
    private readonly FVNWEBAPPContext _db;
    private readonly ICalendarModuleRegistry _registry;
    private readonly IWorkCalendarService _workCalendar;
    private readonly ICalendarDayRuleEngine _ruleEngine;
    private readonly IAuthorizationService _authorization;
    private readonly ILogger<SharedWorkCalendarService> _logger;

    public SharedWorkCalendarService(
        FVNWEBAPPContext db,
        ICalendarModuleRegistry registry,
        IWorkCalendarService workCalendar,
        ICalendarDayRuleEngine ruleEngine,
        IAuthorizationService authorization,
        ILogger<SharedWorkCalendarService> logger)
    {
        _db = db;
        _registry = registry;
        _workCalendar = workCalendar;
        _ruleEngine = ruleEngine;
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

        var employee = await _db.Employees
            .AsNoTracking()
            .Where(x => x.IsActive != false
                && x.EmployeeCode != null
                && x.EmployeeCode.Trim() == normalizedEmployeeCode)
            .Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.DeptCode,
                x.PositionCode
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhân viên '{normalizedEmployeeCode}'.");

        var resolvedEmployeeCode = employee.EmployeeCode?.Trim() ?? normalizedEmployeeCode;

        if (!string.Equals(actor.EmployeeCode?.Trim(), resolvedEmployeeCode, StringComparison.OrdinalIgnoreCase)
            && !await _authorization.CanAccessAsync(
                actor,
                SecurityFunctionCodes.CalendarView,
                resolvedEmployeeCode,
                null,
                cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có Calendar.View hoặc ManagedScope tới nhân viên này.");
        }

        var policies = await _db.CalendarModulePolicies
            .AsNoTracking()
            .Where(x => x.IsActive != false && x.IsEnabled)
            .ToDictionaryAsync(x => x.ModuleCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var context = new CalendarContext(
            EmployeeId: employee.Id,
            UserId: userId,
            From: from,
            To: to,
            AllowedModules: allowedModules,
            DeptCode: employee.DeptCode,
            PositionCode: employee.PositionCode,
            EmployeeCode: resolvedEmployeeCode);

        var workCalendar = await _workCalendar.GetAsync(
            resolvedEmployeeCode,
            employee.DeptCode,
            employee.PositionCode,
            from.ToDateTime(TimeOnly.MinValue),
            to.ToDateTime(TimeOnly.MinValue),
            cancellationToken);

        var results = new List<IReadOnlyList<CalendarItemDto>>();

        foreach (var provider in _registry.Providers
            .Where(x => policies.ContainsKey(x.ModuleCode)
                && (allowedModules is null || allowedModules.Contains(x.ModuleCode))))
        {
            try
            {
                results.Add(await provider.GetItemsAsync(context, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Calendar provider failed. ModuleCode={ModuleCode}, EmployeeCode={EmployeeCode}, EmployeeId={EmployeeId}, From={From}, To={To}",
                    provider.ModuleCode,
                    context.EmployeeCode,
                    context.EmployeeId,
                    context.From,
                    context.To);
            }
        }

        var items = results
            .SelectMany(x => x)
            .OrderBy(x => x.WorkDate)
            .ThenByDescending(x => x.RequiresAction)
            .ThenByDescending(x => x.Severity)
            .ThenBy(x => policies.TryGetValue(x.ModuleCode, out var policy) ? policy.Priority : 100)
            .ThenBy(x => x.ModuleCode)
            .ToArray();

        var itemsByDate = items
            .GroupBy(x => x.WorkDate)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<CalendarItemDto>)x.ToArray());

        var days = new List<CalendarDayDto>();

        foreach (var baseDay in workCalendar.Days.OrderBy(x => x.Date))
        {
            var date = DateOnly.FromDateTime(baseDay.Date.Date);
            var dateItems = itemsByDate.TryGetValue(date, out var sourceItems)
                ? sourceItems
                : Array.Empty<CalendarItemDto>();

            var registrations = workCalendar.Events
                .Where(x => x.ModuleCode is "LEAVE" or "OT" or "TRIP"
                    && CalendarEventDateMatcher.CoversDate(x, baseDay.Date))
                .Where(x => allowedModules is null || allowedModules.Contains(x.ModuleCode))
                .OrderBy(x => x.ModuleCode)
                .ThenBy(x => x.Start)
                .Select(x =>
                {
                    var projection = dateItems
                        .Where(i => string.Equals(i.ModuleCode, x.ModuleCode, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault(i =>
                            string.Equals(i.SourceId, x.SourceId, StringComparison.OrdinalIgnoreCase))
                        ?? GetSingleModuleProjection(dateItems, x.ModuleCode);

                    return new CalendarRegistrationDto
                    {
                        ModuleCode = x.ModuleCode,
                        RequestId = x.RequestId,
                        SubTypeCode = x.SubTypeCode,
                        Title = x.Title,
                        Status = x.Status,
                        Start = x.Start,
                        End = x.End,
                        IsHalfDay = x.IsHalfDay,
                        DayValue = x.DayValue <= 0 ? 1m : x.DayValue,
                        ActionId = projection?.ActionId ?? x.ActionId,
                        DetailRoute = projection?.DetailRoute ?? x.DetailRoute
                    };
                })
                .ToArray();

            var attendanceItem = dateItems.FirstOrDefault(x =>
                string.Equals(x.ModuleCode, "ATTENDANCE", StringComparison.OrdinalIgnoreCase));

            CalendarAttendanceInfoDto? attendance = null;
            CalendarShiftInfoDto? shift = null;

            if (attendanceItem is not null)
            {
                shift = new CalendarShiftInfoDto
                {
                    ShiftId = attendanceItem.ShiftId,
                    ShiftAbbr = attendanceItem.ShiftAbbr,
                    RequiredMinutes = attendanceItem.RequiredMinutes
                };

                attendance = new CalendarAttendanceInfoDto
                {
                    CheckIn = attendanceItem.CheckIn,
                    CheckOut = attendanceItem.CheckOut,
                    WorkMinutes = attendanceItem.WorkMinutes,
                    RequiredMinutes = attendanceItem.RequiredMinutes,
                    ActualHours = attendanceItem.ActualHours,
                    RequiredHours = attendanceItem.RequiredHours,
                    ActualOtMinutes = attendanceItem.ActualOtMinutes,
                    RecognizedOtMinutes = attendanceItem.RecognizedOtMinutes,
                    HasActual = attendanceItem.HasActual,
                    HasActualOt = attendanceItem.HasActualOt,
                    IsNumericVariance = attendanceItem.IsNumericVariance,
                    DisplayValue = attendanceItem.Marker,
                    SourceId = attendanceItem.SourceId,
                    ActionId = attendanceItem.ActionId,
                    DetailRoute = attendanceItem.DetailRoute
                };
            }

            var provisional = new CalendarDayDto
            {
                Date = date,
                WorkYear = baseDay.WorkYear,
                IsToday = !baseDay.IsFuture && !baseDay.IsPast,
                IsFuture = baseDay.IsFuture,
                IsPast = baseDay.IsPast,
                IsWeekend = baseDay.IsWeekend,
                IsWorkingDay = baseDay.IsWorkingDay,
                Holiday = string.IsNullOrWhiteSpace(baseDay.HolidayName)
                    ? null
                    : new CalendarHolidayInfoDto
                    {
                        Code = baseDay.HolidayCode,
                        Name = baseDay.HolidayName
                    },
                Shift = shift,
                Attendance = attendance,
                Registrations = registrations,
                CanRegisterLeave = baseDay.CanRegisterLeave
                    && !registrations.Any(x => x.ModuleCode == "LEAVE"),
                CanRegisterOT = baseDay.CanRegisterOT
                    && !registrations.Any(x => x.ModuleCode == "OT"),
                CanRegisterTrip = baseDay.CanRegisterTrip
                    && !registrations.Any(x => x.ModuleCode == "TRIP"),
                AvailabilityNote = baseDay.AvailabilityNote
            };

            var issues = _ruleEngine.Evaluate(
                new CalendarDayRuleContext(provisional, dateItems));

            days.Add(new CalendarDayDto
            {
                Date = provisional.Date,
                WorkYear = provisional.WorkYear,
                IsToday = provisional.IsToday,
                IsFuture = provisional.IsFuture,
                IsPast = provisional.IsPast,
                IsWeekend = provisional.IsWeekend,
                IsWorkingDay = provisional.IsWorkingDay,
                Holiday = provisional.Holiday,
                Shift = provisional.Shift,
                Attendance = provisional.Attendance,
                Registrations = provisional.Registrations,
                Issues = issues,
                CanRegisterLeave = provisional.CanRegisterLeave,
                CanRegisterOT = provisional.CanRegisterOT,
                CanRegisterTrip = provisional.CanRegisterTrip,
                AvailabilityNote = BuildAvailabilityNote(provisional)
            });
        }

        var opportunities = days
            .SelectMany(day => BuildRegistrationOpportunities(day, allowedModules))
            .ToArray();

        var alerts = days
            .SelectMany(day => day.Issues.Select(issue => new CalendarAlertItemDto
            {
                WorkDate = day.Date,
                ModuleCode = string.IsNullOrWhiteSpace(issue.ModuleCode) ? issue.Code : issue.ModuleCode,
                Severity = issue.Severity >= 3
                    ? "Critical"
                    : issue.Severity == 2
                        ? "Warning"
                        : "Attention",
                Summary = string.IsNullOrWhiteSpace(issue.Summary)
                    ? issue.Title
                    : issue.Summary!,
                RequiresAction = issue.Actions.Count > 0,
                ActionId = issue.ActionId,
                DetailRoute = issue.DetailRoute
                    ?? issue.Actions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.DetailRoute))?.DetailRoute,
                SourceId = issue.SourceId ?? day.Date.ToString("yyyy-MM-dd")
            }))
            .OrderByDescending(x => x.WorkDate)
            .ThenByDescending(x => x.Severity)
            .ToArray();

        return new CalendarMonthDto
        {
            From = from,
            To = to,
            Items = items,
            Days = days,
            Alerts = alerts,
            RegistrationOpportunities = opportunities
        };
    }

    private static CalendarItemDto? GetSingleModuleProjection(
        IReadOnlyList<CalendarItemDto> items,
        string moduleCode)
    {
        var matches = items
            .Where(i => string.Equals(i.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }

    public async Task<IReadOnlyList<CalendarAlertItemDto>> GetAlertsAsync(
        string employeeCode,
        int userId,
        DateOnly from,
        DateOnly to,
        IReadOnlySet<string>? allowedModules = null,
        CancellationToken cancellationToken = default)
    {
        var result = await GetMonthAsync(
            employeeCode,
            userId,
            from,
            to,
            allowedModules,
            cancellationToken);

        return result.Alerts;
    }

    private static string? BuildAvailabilityNote(CalendarDayDto day)
    {
        if (day.Holiday is not null)
            return "Ngày nghỉ công ty — chỉ đăng ký OT.";

        if (day.IsToday)
            return "Hôm nay không mở đăng ký.";

        if (day.IsPast)
            return "Ngày đã qua — chỉ xem và xử lý chênh lệch.";

        if (day.WorkYear is null)
            return "Chưa cấu hình năm làm việc.";

        if (!day.IsWorkingDay)
            return "Không phải ngày làm việc theo lịch công ty.";

        if (!day.CanRegisterLeave && !day.CanRegisterOT && !day.CanRegisterTrip && day.Registrations.Count > 0)
            return "Đã có đăng ký trong ngày.";

        return day.AvailabilityNote;
    }

    private static IEnumerable<CalendarRegistrationOpportunityDto> BuildRegistrationOpportunities(
        CalendarDayDto day,
        IReadOnlySet<string>? allowedModules)
    {
        if (!day.IsFuture)
            yield break;

        if (day.CanRegisterLeave && IsAllowed("LEAVE", allowedModules))
        {
            yield return new CalendarRegistrationOpportunityDto
            {
                WorkDate = day.Date,
                ModuleCode = "LEAVE",
                Title = "Đăng ký nghỉ",
                Route = $"/leave/create?date={day.Date:yyyy-MM-dd}",
                IsEnabled = true
            };
        }

        if (day.CanRegisterOT && IsAllowed("OT", allowedModules))
        {
            yield return new CalendarRegistrationOpportunityDto
            {
                WorkDate = day.Date,
                ModuleCode = "OT",
                Title = "Đăng ký OT",
                Route = $"/ot/create?date={day.Date:yyyy-MM-dd}",
                IsEnabled = true
            };
        }

        if (day.CanRegisterTrip && IsAllowed("TRIP", allowedModules))
        {
            yield return new CalendarRegistrationOpportunityDto
            {
                WorkDate = day.Date,
                ModuleCode = "TRIP",
                Title = "Đăng ký công tác",
                Route = $"/trip/create?date={day.Date:yyyy-MM-dd}",
                IsEnabled = true
            };
        }
    }

    private static bool IsAllowed(string moduleCode, IReadOnlySet<string>? allowedModules) =>
        allowedModules is null || allowedModules.Contains(moduleCode);
}