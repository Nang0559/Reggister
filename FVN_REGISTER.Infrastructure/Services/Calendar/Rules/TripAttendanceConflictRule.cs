using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar.Rules;

public sealed class TripAttendanceConflictRule : ICalendarDayRule
{
    public int Order => 40;

    public IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context)
    {
        if (context.Day.Attendance?.HasActual != true)
            return Array.Empty<CalendarIssueDto>();

        var trip = context.Day.Registrations.FirstOrDefault(x =>
            x.ModuleCode == "TRIP"
            && IsApproved(x.Status)
            && !x.IsHalfDay
            && x.DayValue >= 1m);

        if (trip is null)
            return Array.Empty<CalendarIssueDto>();

        var attendanceItem = context.SourceItems.FirstOrDefault(x => x.ModuleCode == "ATTENDANCE");

        return new[]
        {
            new CalendarIssueDto
            {
                Code = "TRIP_HAS_ATTENDANCE",
                ModuleCode = "TRIP",
                Severity = 3,
                Marker = "?",
                Title = "Có chấm công trong ngày công tác",
                Summary = "Đã đăng ký công tác cả ngày nhưng phát sinh giờ đi làm.",
                SourceId = attendanceItem?.SourceId ?? context.Day.Date.ToString("yyyy-MM-dd"),
                ActionId = attendanceItem?.ActionId,
                DetailRoute = attendanceItem?.DetailRoute,
                RequestId = trip.RequestId,
                Actions = new[]
                {
                    new CalendarActionOptionDto
                    {
                        Code = "OPEN_HR_FEEDBACK",
                        Title = "Phản hồi nhân sự",
                        Description = "Giải thích chênh lệch giữa đăng ký công tác và chấm công.",
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
}