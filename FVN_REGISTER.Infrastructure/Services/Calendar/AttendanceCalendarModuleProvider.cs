using System.Globalization;
using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Entities.WorkCalendar;
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
                a.ShiftId,
                a.ShiftAbbr,
                a.AttendanceDisplayValue,
                a.CheckInTime,
                a.CheckOutTime,
                a.WorkMinutesDay,
                a.WorkMinutesNight,
                a.OTMinutesDay,
                a.OTMinutesNight,
                a.OTMinutesDayTC,
                a.OTMinutesNightTC,
                a.OTRecognizedMinutesDay,
                a.OTRecognizedMinutesNight,
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

        var reconciliationByDate = await _db.ExecutionReconciliations
            .AsNoTracking()
            .Where(x => x.IsActive != false
                && x.ModuleCode == "ATTENDANCE"
                && x.EmployeeId == context.EmployeeId
                && x.WorkDate >= context.From
                && x.WorkDate <= context.To)
            .ToDictionaryAsync(x => x.WorkDate, cancellationToken);

        return rows.Select(row => Map(row, reconciliationByDate.TryGetValue(row.WorkDate, out var reconciliation) ? reconciliation : null)).ToArray();
    }

    private static CalendarItemDto Map(
        AttendanceCalendarRow row,
        F03ExecutionReconciliation? reconciliation)
    {
        var isLeave = (row.LeaveTotal ?? 0m) > 0m;
        var isHoliday = row.HrmHoliday == true || row.HrmEmployeeHoliday == true;
        var hasCheckIn = row.CheckInTime.HasValue;
        var hasCheckOut = row.CheckOutTime.HasValue;
        var lateMinutes = row.LateMinutesDay + row.LateMinutesNight;
        var earlyMinutes = row.EarlyLeaveMinutesDay + row.EarlyLeaveMinutesNight;
        var requiredMinutes = Math.Max(row.RequiredMinutes, 0);
        var workedMinutes = Math.Max(0, row.WorkMinutesDay) + Math.Max(0, row.WorkMinutesNight);
        var actualOtMinutes = Math.Max(0, row.OTMinutesDay)
            + Math.Max(0, row.OTMinutesNight)
            + Math.Max(0, row.OTMinutesDayTC)
            + Math.Max(0, row.OTMinutesNightTC);
        var recognizedOtMinutes = Math.Max(0, row.OTRecognizedMinutesDay)
            + Math.Max(0, row.OTRecognizedMinutesNight);

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

        if (reconciliation is not null)
        {
            status = reconciliation.ReconciliationStatus;
            severity = status switch
            {
                "Mismatch" => (byte)3,
                "AwaitingConfirmation" => (byte)2,
                _ => (byte)0
            };
            requiresAction = reconciliation.RequiresConfirmation
                && !string.Equals(status, "Resolved", StringComparison.OrdinalIgnoreCase);
            summary = BuildReconciliationSummary(row, reconciliation, lateMinutes, earlyMinutes, workedMinutes);
        }

        return new CalendarItemDto
        {
            WorkDate = row.WorkDate,
            ModuleCode = "ATTENDANCE",
            SourceType = reconciliation?.SourceType ?? "ATTENDANCE",
            StatusCode = status,
            Marker = reconciliation?.ReconciliationStatus switch
            {
                "Mismatch" => "?",
                "AwaitingConfirmation" => "?",
                "Resolved" => "OK",
                _ => string.IsNullOrWhiteSpace(row.AttendanceDisplayValue) ? null : row.AttendanceDisplayValue
            },
            Summary = summary,
            Severity = severity,
            RequiresAction = requiresAction,
            InteractionType = requiresAction && reconciliation?.ActionId != null ? "CONFIRMATION" : reconciliation != null ? "DETAIL" : "INFO",
            ActionId = reconciliation?.ActionId,
            DetailRoute = reconciliation is null ? null : $"/execution?reconciliationId={reconciliation.Id}",
            SourceId = reconciliation?.Id.ToString() ?? $"{row.HrmEmployeeId}:{row.WorkDate:yyyyMMdd}",
            ShiftId = row.ShiftId,
            ShiftAbbr = row.ShiftAbbr,
            CheckIn = row.CheckInTime,
            CheckOut = row.CheckOutTime,
            WorkMinutes = workedMinutes,
            RequiredMinutes = requiredMinutes,
            ActualOtMinutes = actualOtMinutes,
            RecognizedOtMinutes = recognizedOtMinutes,
            ActualHours = workedMinutes / 60m,
            RequiredHours = requiredMinutes > 0 ? requiredMinutes / 60m : null,
            HasActual = hasCheckIn || hasCheckOut || workedMinutes > 0,
            HasActualOt = actualOtMinutes > 0 || recognizedOtMinutes > 0,
            IsNumericVariance = decimal.TryParse(
                row.AttendanceDisplayValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out _)
                && requiredMinutes > 0
                && hasCheckIn
                && hasCheckOut
        };
    }

    private static string BuildReconciliationSummary(
        AttendanceCalendarRow row,
        F03ExecutionReconciliation reconciliation,
        int lateMinutes,
        int earlyMinutes,
        int workedMinutes)
    {
        if (string.Equals(reconciliation.ReconciliationStatus, "Mismatch", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(reconciliation.PlannedState, "NONE", StringComparison.OrdinalIgnoreCase)
                && (row.CheckInTime.HasValue || row.CheckOutTime.HasValue))
                return $"Có chấm công ngoài kế hoạch{BuildTimeSuffix(row, workedMinutes)}";

            if (row.RequiredMinutes > 0 && (!row.CheckInTime.HasValue || !row.CheckOutTime.HasValue))
                return BuildSummary("MISSING", row, lateMinutes, earlyMinutes, workedMinutes);

            if (lateMinutes > 0 || earlyMinutes > 0)
                return BuildSummary("EXCEPTION", row, lateMinutes, earlyMinutes, workedMinutes);
        }

        if (string.Equals(reconciliation.ReconciliationStatus, "AwaitingConfirmation", StringComparison.OrdinalIgnoreCase))
            return $"Đang chờ xác nhận{BuildTimeSuffix(row, workedMinutes)}";

        if (string.Equals(reconciliation.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            return $"Đã giải quyết{BuildTimeSuffix(row, workedMinutes)}";

        return BuildSummary("PRESENT", row, lateMinutes, earlyMinutes, workedMinutes);
    }

    private static string BuildTimeSuffix(AttendanceCalendarRow row, int workedMinutes)
    {
        if (row.CheckInTime.HasValue && row.CheckOutTime.HasValue)
        {
            var range = $" {row.CheckInTime.Value:HH:mm} - {row.CheckOutTime.Value:HH:mm}";
            return workedMinutes > 0 ? $"{range} ({workedMinutes / 60d:0.##} giờ)" : range;
        }

        if (row.CheckInTime.HasValue) return $" Check-in {row.CheckInTime.Value:HH:mm}";
        if (row.CheckOutTime.HasValue) return $" Check-out {row.CheckOutTime.Value:HH:mm}";
        return string.Empty;
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
        public int? ShiftId { get; set; }
        public string? ShiftAbbr { get; set; }
        public string? AttendanceDisplayValue { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int WorkMinutesDay { get; set; }
        public int WorkMinutesNight { get; set; }
        public int OTMinutesDay { get; set; }
        public int OTMinutesNight { get; set; }
        public int OTMinutesDayTC { get; set; }
        public int OTMinutesNightTC { get; set; }
        public int OTRecognizedMinutesDay { get; set; }
        public int OTRecognizedMinutesNight { get; set; }
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
