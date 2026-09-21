using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class WorkCalendarService : IWorkCalendarService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuthorizationService _authorization;
    private readonly ICurrentUserService _currentUser;

    public WorkCalendarService(IUnitOfWork uow, IAuthorizationService authorization, ICurrentUserService currentUser)
    {
        _uow = uow;
        _authorization = authorization;
        _currentUser = currentUser;
    }

    public async Task<WorkCalendarDto> GetAsync(
        string employeeCode,
        string? deptCode,
        string? positionCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var start = from.Date;
        var end = to.Date < start ? start : to.Date;

        var user = _currentUser.GetCurrentUser();
        if (user == null || !string.Equals(user.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Calendar chỉ được truy vấn cho người dùng hiện tại.");

        var canLeave = await _authorization.HasAsync(user, SecurityFunctionCodes.LeaveView, ct);
        var canOt = await _authorization.HasAsync(user, SecurityFunctionCodes.OTView, ct);
        var canTrip = await _authorization.HasAsync(user, SecurityFunctionCodes.TripView, ct);

        var workYears = await _uow.Repository<F03WorkYear>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true
                && x.StartDate <= end
                && x.EndDate >= start)
            .OrderBy(x => x.WorkYear)
            .ToListAsync(ct);

        var holidays = await _uow.Repository<F03CompanyHoliday>().Query()
            .AsNoTracking()
            .Where(x => x.HolidayDate >= start && x.HolidayDate <= end)
            .OrderBy(x => x.HolidayDate)
            .ToListAsync(ct);

        var leave = canLeave
            ? await _uow.Repository<VF03LeaveRequest>().Query().AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true
                    && x.EndDate >= start && x.StartDate <= end
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .ToListAsync(ct)
            : new List<VF03LeaveRequest>();

        var ot = canOt
            ? await _uow.Repository<VF03OTRequest>().Query().AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true
                    && x.OTDate >= start && x.OTDate <= end
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .ToListAsync(ct)
            : new List<VF03OTRequest>();

        var trips = canTrip
            ? await _uow.Repository<F03TripRequest>().Query().AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true
                    && x.EndDate >= start && x.StartDate <= end
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .ToListAsync(ct)
            : new List<F03TripRequest>();

        var holidayByDate = holidays
            .GroupBy(x => x.HolidayDate.Date)
            .ToDictionary(x => x.Key, x => x.First());

        var workYearByDate = new Dictionary<DateTime, F03WorkYear>();
        foreach (var workYear in workYears)
        {
            var yearStart = workYear.StartDate.Date < start ? start : workYear.StartDate.Date;
            var yearEnd = workYear.EndDate.Date > end ? end : workYear.EndDate.Date;

            for (var date = yearStart; date <= yearEnd; date = date.AddDays(1))
                workYearByDate[date] = workYear;
        }

        var result = new WorkCalendarDto { From = start, To = end };

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var holiday = holidayByDate.GetValueOrDefault(date.Date);
            var workYear = workYearByDate.GetValueOrDefault(date.Date);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var working = workYear != null && !weekend && holiday == null;

            var day = new WorkCalendarDayDto
            {
                Date = date,
                WorkYear = workYear?.WorkYear,
                IsWeekend = weekend,
                IsWorkingDay = working,
                HolidayName = holiday?.Description,
                HolidayCode = holiday == null ? null : $"HOL-{holiday.HolidayDate:yyyyMMdd}",
                CanRegisterLeave = working,
                CanRegisterOT = workYear != null && (!weekend || holiday != null),
                CanRegisterTrip = true,
                AvailabilityNote = holiday != null
                    ? $"Nghỉ công ty: {holiday.Description}"
                    : workYear == null
                        ? "Chưa cấu hình năm làm việc."
                        : weekend ? "Cuối tuần" : null
            };

            result.Days.Add(day);
        }

        foreach (var h in holidays)
        {
            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"COMPANY_{h.Id}",
                ModuleCode = "COMPANY",
                EventType = "HOLIDAY",
                Title = string.IsNullOrWhiteSpace(h.Description) ? "Nghỉ công ty" : h.Description,
                Start = h.HolidayDate.Date,
                End = h.HolidayDate.Date.AddDays(1),
                Status = "Holiday",
                IsReadOnly = true
            });
        }

        foreach (var x in leave)
        {
            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"LEAVE_{x.Id}",
                ModuleCode = "LEAVE",
                EventType = "LEAVE",
                Title = string.IsNullOrWhiteSpace(x.LeaveTypeName) ? "Nghỉ phép" : x.LeaveTypeName,
                Start = x.StartDate.Date,
                End = x.EndDate.Date.AddDays(1),
                Status = x.RequestStatus.ToString(),
                IsReadOnly = true,
                RequestId = x.Id
            });
        }

        foreach (var x in ot)
        {
            var startTime = x.StartTime;
            var endTime = x.EndTime;
            var s = x.OTDate.Date.Add(startTime);
            var e = x.OTDate.Date.Add(endTime);
            if (e <= s) e = e.AddDays(1);

            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"OT_{x.Id}",
                ModuleCode = "OT",
                EventType = "OT",
                Title = $"OT {(x.TotalOTHours ?? 0):0.#}h",
                Start = s,
                End = e,
                Status = x.RequestStatus.ToString(),
                IsReadOnly = true,
                RequestId = x.Id
            });
        }

        foreach (var x in trips)
        {
            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"TRIP_{x.Id}",
                ModuleCode = "TRIP",
                EventType = "TRIP",
                Title = string.IsNullOrWhiteSpace(x.Destination) ? "Công tác" : $"Công tác: {x.Destination}",
                Start = x.StartDate.Date,
                End = x.EndDate.Date.AddDays(1),
                Status = x.RequestStatus.ToString(),
                IsReadOnly = true,
                RequestId = x.Id
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<CalendarRegistrationOpportunityDto>> GetRegistrationOpportunitiesAsync(
        string employeeCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var calendar = await GetAsync(employeeCode, null, null, from.Date, to.Date, ct);
        var result = new List<CalendarRegistrationOpportunityDto>();

        foreach (var day in calendar.Days)
        {
            var date = day.Date.Date;
            var hasLeave = calendar.Events.Any(x => x.ModuleCode == "LEAVE" && x.Start.Date <= date && x.End.Date > date);
            var hasOt = calendar.Events.Any(x => x.ModuleCode == "OT" && x.Start.Date <= date && x.End.Date > date);
            var hasTrip = calendar.Events.Any(x => x.ModuleCode == "TRIP" && x.Start.Date <= date && x.End.Date > date);

            // Existing actionable data takes precedence over creating a new registration.
            if (hasLeave || hasOt || hasTrip)
                continue;

            if (day.CanRegisterLeave)
            {
                result.Add(new CalendarRegistrationOpportunityDto
                {
                    WorkDate = DateOnly.FromDateTime(date),
                    ModuleCode = "LEAVE",
                    Title = "Đăng ký nghỉ",
                    Route = $"/leave/create?date={date:yyyy-MM-dd}",
                    IsEnabled = true
                });
            }

            if (day.CanRegisterOT)
            {
                result.Add(new CalendarRegistrationOpportunityDto
                {
                    WorkDate = DateOnly.FromDateTime(date),
                    ModuleCode = "OT",
                    Title = "Đăng ký OT",
                    Route = $"/ot/create?date={date:yyyy-MM-dd}",
                    IsEnabled = true
                });
            }

            if (day.CanRegisterTrip)
            {
                result.Add(new CalendarRegistrationOpportunityDto
                {
                    WorkDate = DateOnly.FromDateTime(date),
                    ModuleCode = "TRIP",
                    Title = "Đăng ký công tác",
                    Route = $"/trip/create?date={date:yyyy-MM-dd}",
                    IsEnabled = true
                });
            }
        }

        return result;
    }

    public async Task<CalendarAvailabilityDto> GetAvailabilityAsync(
        string employeeCode,
        DateTime date,
        CancellationToken ct = default)
    {
        var day = date.Date;
        var calendar = await GetAsync(employeeCode, null, null, day, day, ct);
        var item = calendar.Days.First();

        var warnings = new List<string>();
        if (item.IsWeekend) warnings.Add("Ngày cuối tuần.");
        if (!string.IsNullOrWhiteSpace(item.HolidayName))
            warnings.Add($"Ngày nghỉ công ty: {item.HolidayName}");

        if (calendar.Events.Any(x => x.ModuleCode == "LEAVE" && x.Start.Date <= day && x.End.Date > day))
            warnings.Add("Đã có đăng ký nghỉ trong ngày.");
        if (calendar.Events.Any(x => x.ModuleCode == "OT" && x.Start.Date <= day && x.End.Date > day))
            warnings.Add("Đã có đăng ký OT trong ngày.");
        if (calendar.Events.Any(x => x.ModuleCode == "TRIP" && x.Start.Date <= day && x.End.Date > day))
            warnings.Add("Đã có đăng ký công tác trong ngày.");

        return new CalendarAvailabilityDto
        {
            Date = day,
            CanRegisterLeave = item.CanRegisterLeave,
            CanRegisterOT = item.CanRegisterOT,
            CanRegisterTrip = item.CanRegisterTrip,
            Warnings = warnings
        };
    }
}