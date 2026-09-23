using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar.Rules;

public sealed class LeaveAttendanceConflictRule : ICalendarDayRule
{
    public int Order => 30;

    public IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context)
    {
        if (context.Day.Attendance?.HasActual != true)
            return Array.Empty<CalendarIssueDto>();

        var conflicts = context.Day.Registrations
            .Where(x => x.ModuleCode == "LEAVE"
                && IsApproved(x.Status)
                && !x.IsHalfDay
                && x.DayValue >= 1m
                && IsPOrCompensatory(x.SubTypeCode))
            .ToList();

        if (conflicts.Count == 0)
            return Array.Empty<CalendarIssueDto>();

        var attendanceItem = context.SourceItems.FirstOrDefault(x => x.ModuleCode == "ATTENDANCE");

        return new[]
        {
            new CalendarIssueDto
            {
                Code = "LEAVE_HAS_ATTENDANCE",
                ModuleCode = "LEAVE",
                Severity = 3,
                Marker = "?",
                Title = "Có chấm công trong ngày nghỉ",
                Summary = $"Đã đăng ký {string.Join(", ", conflicts.Select(x => x.SubTypeCode).Distinct(StringComparer.OrdinalIgnoreCase))} nhưng phát sinh giờ đi làm.",
                SourceId = attendanceItem?.SourceId ?? context.Day.Date.ToString("yyyy-MM-dd"),
                ActionId = attendanceItem?.ActionId,
                DetailRoute = attendanceItem?.DetailRoute,
                RequestId = conflicts.First().RequestId,
                Actions = new[]
                {
                    new CalendarActionOptionDto
                    {
                        Code = "OPEN_HR_FEEDBACK",
                        Title = "Phản hồi nhân sự",
                        Description = "Giải thích lý do ngày nghỉ vẫn phát sinh chấm công.",
                        Kind = "FORM",
                        DetailRoute = attendanceItem?.DetailRoute,
                        RequiresReason = true,
                        RequiresAttachment = true
                    }
                }
            }
        };
    }

    private static bool IsApproved(string? status) =>
        string.Equals(status?.Trim(), "Approved", StringComparison.OrdinalIgnoreCase);

    private static bool IsPOrCompensatory(string? code) =>
        code?.Trim().ToUpperInvariant() is "P" or "NB";
}