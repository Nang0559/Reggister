using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
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
        if (user == null || string.IsNullOrWhiteSpace(user.EmployeeCode))
            throw new UnauthorizedAccessException("Calendar chỉ được truy vấn khi phiên đăng nhập có nhân viên.");

        var normalizedEmployeeCode = employeeCode.Trim();
        if (normalizedEmployeeCode.Length == 0)
            throw new ArgumentException("Mã nhân viên là bắt buộc.", nameof(employeeCode));

        var sameEmployee = string.Equals(
            user.EmployeeCode.Trim(),
            normalizedEmployeeCode,
            StringComparison.OrdinalIgnoreCase);

        if (!sameEmployee
            && !await _authorization.CanAccessAsync(
                user,
                SecurityFunctionCodes.CalendarView,
                normalizedEmployeeCode,
                null,
                ct))
        {
            throw new UnauthorizedAccessException("Bạn không có Calendar.View hoặc ManagedScope tới nhân viên này.");
        }

        var canViewLeave = await _authorization.HasAsync(user, SecurityFunctionCodes.LeaveView, ct);
        var canViewOt = await _authorization.HasAsync(user, SecurityFunctionCodes.OTView, ct);
        var canViewTrip = await _authorization.HasAsync(user, SecurityFunctionCodes.TripView, ct);

        // Reading a managed employee's calendar and being allowed to create a
        // registration for that employee are different capabilities. Calendar
        // must expose registration opportunities only when the caller has the
        // corresponding Create permission for the target employee.
        var canCreateLeave = await _authorization.CanAccessAsync(
            user, SecurityFunctionCodes.LeaveCreate, normalizedEmployeeCode, null, ct);
        var canCreateOt = await _authorization.CanAccessAsync(
            user, SecurityFunctionCodes.OTCreate, normalizedEmployeeCode, null, ct);
        var canCreateTrip = await _authorization.CanAccessAsync(
            user, SecurityFunctionCodes.TripCreate, normalizedEmployeeCode, null, ct);

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

        var leave = canViewLeave
            ? await _uow.Repository<VF03LeaveRequest>().Query().AsNoTracking()
                .Where(x => x.EmployeeCode == normalizedEmployeeCode && x.IsActive == true
                    && x.EndDate >= start && x.StartDate <= end
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .ToListAsync(ct)
            : new List<VF03LeaveRequest>();

        var leaveIds = leave.Select(x => x.Id).Where(x => x > 0).Distinct().ToList();
        var leaveDetails = leaveIds.Count == 0
            ? new List<VF03LeaveRequestDetail>()
            : await _uow.Repository<VF03LeaveRequestDetail>().Query().AsNoTracking()
                .Where(x => leaveIds.Contains(x.LeaveId)
                    && x.LeaveDate >= DateOnly.FromDateTime(start)
                    && x.LeaveDate <= DateOnly.FromDateTime(end))
                .OrderBy(x => x.LeaveDate)
                .ToListAsync(ct);

        var ot = canViewOt
            ? await _uow.Repository<VF03OTRequest>().Query().AsNoTracking()
                .Where(x => x.EmployeeCode == normalizedEmployeeCode && x.IsActive == true
                    && x.OTDate >= start && x.OTDate <= end
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .ToListAsync(ct)
            : new List<VF03OTRequest>();

        var trips = canViewTrip
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
            var future = date.Date > DateTime.Today;
            var past = date.Date < DateTime.Today;
            var working = workYear != null && holiday == null;

            var day = new WorkCalendarDayDto
            {
                Date = date,
                WorkYear = workYear?.WorkYear,
                IsWeekend = weekend,
                IsFuture = future,
                IsPast = past,
                IsWorkingDay = working,
                HolidayName = holiday?.Description,
                HolidayCode = holiday == null ? null : $"HOL-{holiday.HolidayDate:yyyyMMdd}",
                CanRegisterLeave = canCreateLeave && future && workYear != null && holiday == null,
                CanRegisterOT = canCreateOt && future && workYear != null,
                CanRegisterTrip = canCreateTrip && future && workYear != null && holiday == null,
                AvailabilityNote = holiday != null
                    ? $"Ngày nghỉ công ty: {holiday.Description} — chỉ đăng ký OT."
                    : !future
                        ? "Chỉ mở đăng ký từ ngày sau hôm nay."
                        : workYear == null
                            ? "Chưa cấu hình năm làm việc."
                            : null
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
            var requestStart = x.StartDate.Date < start ? start : x.StartDate.Date;
            var requestEnd = x.EndDate.Date > end ? end : x.EndDate.Date;
            var details = leaveDetails
                .Where(d => d.LeaveId == x.Id)
                .ToDictionary(d => d.LeaveDate, d => d);

            for (var date = requestStart; date <= requestEnd; date = date.AddDays(1))
            {
                var detail = details.GetValueOrDefault(DateOnly.FromDateTime(date));
                var dayValue = detail?.DayValue ?? (x.TotalDay < 1m && requestStart == requestEnd ? x.TotalDay : 1m);

                result.Events.Add(new WorkCalendarEventDto
                {
                    Id = $"LEAVE_{x.Id}_{date:yyyyMMdd}",
                    ModuleCode = "LEAVE",
                    EventType = "LEAVE",
                    Title = string.IsNullOrWhiteSpace(detail?.LeaveTypeName ?? x.LeaveTypeName)
                        ? "Nghỉ phép"
                        : (detail?.LeaveTypeName ?? x.LeaveTypeName)!,
                    Start = date,
                    End = date.AddDays(1),
                    Status = x.RequestStatus.ToString(),
                    IsReadOnly = true,
                    RequestId = x.Id,
                    SourceId = $"{x.Id}:{date:yyyyMMdd}",
                    SubTypeCode = detail?.LeaveTypeCode ?? x.LeaveTypeCode,
                    IsHalfDay = detail?.IsHalfDay ?? dayValue < 1m,
                    DayValue = dayValue
                });
            }
        }

        foreach (var x in ot)
        {
            var startTime = x.StartTime;
            var endTime = x.EndTime;
            var s = x.OTDate.Date.Add(startTime.TimeOfDay);
            var e = x.OTDate.Date.Add(endTime.TimeOfDay);
            if (e <= s) e = e.AddDays(1);

            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"OT_{x.Id}",
                ModuleCode = "OT",
                EventType = "OT",
                Title = $"OT {x.TotalOTHours:0.#}h",
                Start = s,
                End = e,
                Status = x.RequestStatus.ToString(),
                IsReadOnly = true,
                RequestId = x.Id,
                SourceId = x.OTCode + ":" + normalizedEmployeeCode,
                DayValue = 1m
            });
        }

        foreach (var x in trips)
        {
            var requestStart = x.StartDate.Date < start ? start : x.StartDate.Date;
            var requestEnd = x.EndDate.Date > end ? end : x.EndDate.Date;

            for (var date = requestStart; date <= requestEnd; date = date.AddDays(1))
            {
                result.Events.Add(new WorkCalendarEventDto
                {
                    Id = $"TRIP_{x.Id}_{date:yyyyMMdd}",
                    ModuleCode = "TRIP",
                    EventType = "TRIP",
                    Title = string.IsNullOrWhiteSpace(x.Destination) ? "Công tác" : $"Công tác: {x.Destination}",
                    Start = date,
                    End = date.AddDays(1),
                    Status = x.RequestStatus.ToString(),
                    IsReadOnly = true,
                    RequestId = x.Id,
                    SourceId = x.TripCode,
                    DayValue = 1m
                });
            }
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
            if (!day.IsFuture)
                continue;

            var hasLeave = calendar.Events.Any(x => x.ModuleCode == "LEAVE" && CalendarEventDateMatcher.CoversDate(x, date));
            var hasOt = calendar.Events.Any(x => x.ModuleCode == "OT" && x.Start.Date <= date && x.End > date);
            var hasTrip = calendar.Events.Any(x => x.ModuleCode == "TRIP" && x.Start.Date <= date && x.End > date);

            if (day.CanRegisterLeave && !hasLeave)
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

            if (day.CanRegisterOT && !hasOt)
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

            if (day.CanRegisterTrip && !hasTrip)
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
        if (!item.IsFuture) warnings.Add("Chỉ mở đăng ký từ ngày sau hôm nay.");
        if (!string.IsNullOrWhiteSpace(item.HolidayName))
            warnings.Add($"Ngày nghỉ công ty: {item.HolidayName} — chỉ OT.");

        if (calendar.Events.Any(x => x.ModuleCode == "LEAVE" && CalendarEventDateMatcher.CoversDate(x, day)))
            warnings.Add("Đã có đăng ký nghỉ trong ngày.");
        if (calendar.Events.Any(x => x.ModuleCode == "OT" && CalendarEventDateMatcher.CoversDate(x, day)))
            warnings.Add("Đã có đăng ký OT trong ngày.");
        if (calendar.Events.Any(x => x.ModuleCode == "TRIP" && x.Start.Date <= day && x.End > day))
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