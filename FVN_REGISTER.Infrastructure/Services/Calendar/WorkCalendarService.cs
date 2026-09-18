using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class WorkCalendarService : IWorkCalendarService
{
    private readonly IUnitOfWork _uow;
    public WorkCalendarService(IUnitOfWork uow)
    {
        _uow = uow;
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

        var holidays = await _uow.Repository<F03CompanyHoliday>().Query()
            .AsNoTracking()
            .Where(x => x.HolidayDate >= start && x.HolidayDate <= end)
            .OrderBy(x => x.HolidayDate)
            .ToListAsync(ct);

        var leave = await _uow.Repository<VF03LeaveRequest>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode
                && x.IsActive == true
                && x.EndDate >= start
                && x.StartDate <= end
                && x.RequestStatus != ApprovalStatus.Cancelled
                && x.RequestStatus != ApprovalStatus.Rejected)
            .ToListAsync(ct);

        var ot = await _uow.Repository<VF03OTRequest>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode
                && x.IsActive == true
                && x.OTDate >= start
                && x.OTDate <= end
                && x.RequestStatus != ApprovalStatus.Cancelled
                && x.RequestStatus != ApprovalStatus.Rejected)
            .ToListAsync(ct);

        var trips = await _uow.Repository<F03TripRequest>().Query()
            .AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode
                && x.IsActive == true
                && x.EndDate >= start
                && x.StartDate <= end
                && x.RequestStatus != ApprovalStatus.Cancelled
                && x.RequestStatus != ApprovalStatus.Rejected)
            .ToListAsync(ct);

        var holidayByDate = holidays
            .GroupBy(x => x.HolidayDate.Date)
            .ToDictionary(x => x.Key, x => x.First());

        var result = new WorkCalendarDto { From = start, To = end };

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var holiday = holidayByDate.GetValueOrDefault(date.Date);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var working = !weekend && holiday == null;

            var day = new WorkCalendarDayDto
            {
                Date = date,
                IsWeekend = weekend,
                IsWorkingDay = working,
                HolidayName = holiday?.Description,
                HolidayCode = holiday == null ? null : $"HOL-{holiday.HolidayDate:yyyyMMdd}",
                CanRegisterLeave = working,
                CanRegisterOT = !weekend || holiday != null,
                CanRegisterTrip = true,
                AvailabilityNote = holiday != null
                    ? $"Nghỉ công ty: {holiday.Description}"
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