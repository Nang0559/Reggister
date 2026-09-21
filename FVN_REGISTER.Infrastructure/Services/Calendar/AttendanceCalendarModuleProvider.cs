using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

/// <summary>
/// Reads the official HRM attendance calculation result directly.
/// Attendance is not an execution-reconciliation projection: normal
/// attendance rows must be visible in Calendar even when no mismatch exists.
/// </summary>
public sealed class AttendanceCalendarModuleProvider : ICalendarModuleProvider
{
    private readonly FVNWEBAPPContext _db;

    public AttendanceCalendarModuleProvider(FVNWEBAPPContext db)
    {
        _db = db;
    }

    public string ModuleCode => "ATTENDANCE";

    public async Task<IReadOnlyList<CalendarItemDto>> GetItemsAsync(
        CalendarContext context,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Database.SqlQuery<AttendanceCalendarRow>($"""
            SELECT
                a.WorkDate,
                a.HrmEmployeeId,
                a.EmployeeCode,
                a.AttendanceDisplayValue,
                a.CheckInTime,
                a.CheckOutTime,
                a.WorkMinutesDay,
                a.WorkMinutesNight,
                a.RequiredMinutes,
                a.LateMinutesDay,
                a.LateMinutesNight,
                a.EarlyLeaveMinutesDay,
                a.EarlyLeaveMinutesNight,
                a.LeaveTotal,
                a.HrmHoliday,
                a.HrmEmployeeHoliday,
                a.HrmBCLyDoNghi,
                a.HrmBCGhiChu
            FROM dbo.F03HrmAttendanceCalculated AS a
            INNER JOIN dbo.F03Employees AS e
                ON LTRIM(RTRIM(e.EmployeeCode)) = LTRIM(RTRIM(a.EmployeeCode))
            WHERE e.Id = {context.EmployeeId}
              AND a.WorkDate >= {context.From}
              AND a.WorkDate <= {context.To}
            ORDER BY a.WorkDate
            """).ToListAsync(cancellationToken);

        return rows.Select(Map).ToArray();
    }

    private static CalendarItemDto Map(AttendanceCalendarRow row)
    {
        var isLeave = (row.LeaveTotal ?? 0m) > 0m;
        var isHoliday = row.HrmHoliday == true || row.HrmEmployeeHoliday == true;
        var hasCheckIn = row.CheckInTime.HasValue;
        var hasCheckOut = row.CheckOutTime.HasValue;
        var lateMinutes = row.LateMinutesDay + row.LateMinutesNight;
        var earlyMinutes = row.EarlyLeaveMinutesDay + row.EarlyLeaveMinutesNight;
        var requiredMinutes = Math.Max(row.RequiredMinutes, 0);
        var workedMinutes = row.WorkMinutesDay + row.WorkMinutesNight;

        byte severity = 0;
        bool requiresAction = false;
        string status;

        if (isLeave)
        {
            status = "LEAVE";
        }
        else if (isHoliday)
        {
            status = "HOLIDAY";
        }
        else if (requiredMinutes > 0 && (!hasCheckIn || !hasCheckOut))
        {
            status = "MISSING";
            severity = 3;
            requiresAction = true;
        }
        else if (lateMinutes > 0 || earlyMinutes > 0)
        {
            status = "EXCEPTION";
            severity = 2;
            requiresAction = true;
        }
        else if (workedMinutes > 0 || hasCheckIn || hasCheckOut)
        {
            status = "PRESENT";
        }
        else
        {
            status = "NO_ATTENDANCE";
        }

        var summary = BuildSummary(
            status,
            row,
            lateMinutes,
            earlyMinutes,
            workedMinutes);

        return new CalendarItemDto
        {
            WorkDate = row.WorkDate,
            ModuleCode = "ATTENDANCE",
            StatusCode = status,
            Marker = string.IsNullOrWhiteSpace(row.AttendanceDisplayValue)
                ? null
                : row.AttendanceDisplayValue,
            Summary = summary,
            Severity = severity,
            RequiresAction = requiresAction,
            ActionId = null,
            DetailRoute = null,
            SourceId = $"{row.HrmEmployeeId}:{row.WorkDate:yyyy-MM-dd}"
        };
    }

    private static string BuildSummary(
        string status,
        AttendanceCalendarRow row,
        int lateMinutes,
        int earlyMinutes,
        int workedMinutes)
    {
        if (status == "LEAVE")
        {
            var reason = row.HrmBCLyDoNghi ?? row.HrmBCGhiChu;
            return string.IsNullOrWhiteSpace(reason)
                ? "Nghỉ phép"
                : $"Nghỉ: {reason}";
        }

        if (status == "HOLIDAY")
            return "Ngày nghỉ";

        if (status == "MISSING")
        {
            if (!row.CheckInTime.HasValue && !row.CheckOutTime.HasValue)
                return "Thiếu Check-in và Check-out";

            return !row.CheckInTime.HasValue
                ? "Thiếu Check-in"
                : "Thiếu Check-out";
        }

        if (status == "EXCEPTION")
        {
            var parts = new List<string>(2);
            if (lateMinutes > 0) parts.Add($"Đi muộn {lateMinutes} phút");
            if (earlyMinutes > 0) parts.Add($"Về sớm {earlyMinutes} phút");
            return string.Join(" • ", parts);
        }

        if (status == "PRESENT")
        {
            if (row.CheckInTime.HasValue && row.CheckOutTime.HasValue)
            {
                var range = $"{row.CheckInTime.Value:HH:mm} - {row.CheckOutTime.Value:HH:mm}";
                return workedMinutes > 0
                    ? $"{range} ({workedMinutes / 60d:0.##} giờ)"
                    : range;
            }

            if (row.CheckInTime.HasValue)
                return $"Check-in {row.CheckInTime.Value:HH:mm}";

            if (row.CheckOutTime.HasValue)
                return $"Check-out {row.CheckOutTime.Value:HH:mm}";

            return "Có chấm công";
        }

        return "Chưa có dữ liệu chấm công";
    }

    private sealed class AttendanceCalendarRow
    {
        public DateOnly WorkDate { get; set; }
        public int HrmEmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? AttendanceDisplayValue { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int WorkMinutesDay { get; set; }
        public int WorkMinutesNight { get; set; }
        public int RequiredMinutes { get; set; }
        public int LateMinutesDay { get; set; }
        public int LateMinutesNight { get; set; }
        public int EarlyLeaveMinutesDay { get; set; }
        public int EarlyLeaveMinutesNight { get; set; }
        public decimal? LeaveTotal { get; set; }
        public bool? HrmHoliday { get; set; }
        public bool? HrmEmployeeHoliday { get; set; }
        public string? HrmBCLyDoNghi { get; set; }
        public string? HrmBCGhiChu { get; set; }
    }
}
