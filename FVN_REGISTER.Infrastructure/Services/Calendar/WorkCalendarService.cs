using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            .Where(x => x.IsActive == true && x.StartDate <= end && x.EndDate >= start)
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
                    && x.RequestStatus == ApprovalStatus.Approved)
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

        var attendance = await LoadLatestAttendanceAsync(employeeCode, start, end, ct);

        var holidayByDate = holidays.GroupBy(x => x.HolidayDate.Date).ToDictionary(x => x.Key, x => x.First());
        var workYearByDate = new Dictionary<DateTime, F03WorkYear>();

        foreach (var workYear in workYears)
        {
            var yearStart = workYear.StartDate.Date < start ? start : workYear.StartDate.Date;
            var yearEnd = workYear.EndDate.Date > end ? end : workYear.EndDate.Date;

            for (var date = yearStart; date <= yearEnd; date = date.AddDays(1))
                workYearByDate[date] = workYear;
        }

        var approvedOtByDate = ot.GroupBy(x => x.OTDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalOTHours));

        var result = new WorkCalendarDto { From = start, To = end };

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var holiday = holidayByDate.GetValueOrDefault(date.Date);
            var workYear = workYearByDate.GetValueOrDefault(date.Date);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var working = workYear != null && !weekend && holiday == null;
            var actual = attendance.GetValueOrDefault(date.Date);
            var approvedOt = approvedOtByDate.GetValueOrDefault(date.Date);

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
                        : weekend ? "Cuối tuần" : null,
                HasAttendance = actual != null,
                ActualCheckIn = actual?.CheckIn,
                ActualCheckOut = actual?.CheckOut,
                ActualWorkHours = actual?.WorkHours ?? 0m,
                ActualOTHours = actual?.OTHours ?? 0m,
                ApprovedOTHours = approvedOt
            };

            if (actual != null && actual.OTHours > 0m)
            {
                if (approvedOt <= 0m)
                {
                    day.AttendanceStatus = "OT_UNREGISTERED";
                    day.AttendanceNote = $"Có {actual.OTHours:0.##} giờ OT thực tế nhưng không có OT được duyệt.";
                }
                else if (Math.Abs(actual.OTHours - approvedOt) > 0.01m)
                {
                    day.AttendanceStatus = "OT_MISMATCH";
                    day.AttendanceNote = $"OT thực tế {actual.OTHours:0.##}h, OT được duyệt {approvedOt:0.##}h.";
                }
                else
                {
                    day.AttendanceStatus = "OT_MATCH";
                    day.AttendanceNote = $"OT thực tế khớp OT được duyệt: {actual.OTHours:0.##}h.";
                }
            }
            else if (actual != null)
            {
                day.AttendanceStatus = "ATTENDANCE";
            }

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
                IsReadOnly = true,
                AllDay = true
            });
        }

        foreach (var day in result.Days.Where(x => x.HasAttendance))
        {
            var color = day.AttendanceStatus switch
            {
                "OT_MATCH" => "#2e7d32",
                "OT_MISMATCH" or "OT_UNREGISTERED" => "#d32f2f",
                _ => "#1976d2"
            };

            var title = $"Công {day.ActualWorkHours:0.##}h";
            if (day.ActualOTHours > 0m)
                title += $" · OT {day.ActualOTHours:0.##}h";
            if (day.ApprovedOTHours > 0m)
                title += $" · Duyệt {day.ApprovedOTHours:0.##}h";

            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"ATT_{day.Date:yyyyMMdd}",
                ModuleCode = "ATTENDANCE",
                EventType = "ATTENDANCE",
                Title = title,
                Start = day.Date,
                End = day.Date.AddDays(1),
                Status = day.AttendanceStatus ?? "ATTENDANCE",
                IsReadOnly = true,
                Color = color,
                BackgroundColor = color,
                BorderColor = color,
                AllDay = true
            });

            if (!string.IsNullOrWhiteSpace(day.AttendanceNote))
            {
                result.Events.Add(new WorkCalendarEventDto
                {
                    Id = $"ATT_NOTE_{day.Date:yyyyMMdd}",
                    ModuleCode = "ATTENDANCE_NOTE",
                    EventType = "ATTENDANCE_NOTE",
                    Title = day.AttendanceNote,
                    Start = day.Date,
                    End = day.Date.AddDays(1),
                    Status = day.AttendanceStatus ?? "ATTENDANCE",
                    IsReadOnly = true,
                    Color = color,
                    BackgroundColor = color,
                    BorderColor = color,
                    AllDay = true
                });
            }
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
            var s = x.OTDate.Date.Add(x.StartTime);
            var e = x.OTDate.Date.Add(x.EndTime);
            if (e <= s) e = e.AddDays(1);

            result.Events.Add(new WorkCalendarEventDto
            {
                Id = $"OT_{x.Id}",
                ModuleCode = "OT",
                EventType = "OT",
                Title = $"OT được duyệt {x.TotalOTHours:0.#}h",
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

    public async Task<CalendarAvailabilityDto> GetAvailabilityAsync(string employeeCode, DateTime date, CancellationToken ct = default)
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
            warnings.Add("Đã có đăng ký OT được duyệt trong ngày.");
        if (item.AttendanceStatus is "OT_UNREGISTERED" or "OT_MISMATCH")
            warnings.Add(item.AttendanceNote!);

        return new CalendarAvailabilityDto
        {
            Date = day,
            CanRegisterLeave = item.CanRegisterLeave,
            CanRegisterOT = item.CanRegisterOT,
            CanRegisterTrip = item.CanRegisterTrip,
            Warnings = warnings
        };
    }

    private async Task<Dictionary<DateTime, AttendanceSnapshot>> LoadLatestAttendanceAsync(
        string employeeCode, DateTime from, DateTime to, CancellationToken ct)
    {
        var pEmployee = new SqlParameter("@EmployeeCode", SqlDbType.NVarChar, 50) { Value = employeeCode };
        var pFrom = new SqlParameter("@FromDate", SqlDbType.Date) { Value = from.Date };
        var pTo = new SqlParameter("@ToDate", SqlDbType.Date) { Value = to.Date };

        const string sql = @"
WITH Latest AS
(
    SELECT a.WorkDate, a.CheckInTime, a.CheckOutTime,
           a.WorkMinutesDay, a.WorkMinutesNight,
           a.OTRecognizedMinutesDay, a.OTRecognizedMinutesNight,
           a.CalculatedAt, a.Id,
           ROW_NUMBER() OVER
           (
               PARTITION BY a.EmployeeCode, a.WorkDate
               ORDER BY a.CalculatedAt DESC, a.Id DESC
           ) AS rn
    FROM dbo.F03HrmAttendanceCalculated a
    WHERE a.EmployeeCode = @EmployeeCode
      AND a.WorkDate BETWEEN @FromDate AND @ToDate
)
SELECT WorkDate,
       CheckInTime AS [CheckIn],
       CheckOutTime AS [CheckOut],
       CAST((ISNULL(WorkMinutesDay,0) + ISNULL(WorkMinutesNight,0)) / 60.0 AS decimal(9,2)) AS WorkHours,
       CAST((ISNULL(OTRecognizedMinutesDay,0) + ISNULL(OTRecognizedMinutesNight,0)) / 60.0 AS decimal(9,2)) AS OTHours
FROM Latest
WHERE rn = 1
ORDER BY WorkDate;";

        var rows = await _uow.SqlQueryRawAsync<AttendanceSnapshot>(sql, ct, pEmployee, pFrom, pTo);
        return rows.GroupBy(x => x.WorkDate.Date).ToDictionary(x => x.Key, x => x.First());
    }

    private sealed class AttendanceSnapshot
    {
        public DateTime WorkDate { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal WorkHours { get; set; }
        public decimal OTHours { get; set; }
    }
}
