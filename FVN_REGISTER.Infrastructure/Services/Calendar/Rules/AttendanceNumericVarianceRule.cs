using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar.Rules;

public sealed class AttendanceNumericVarianceRule : ICalendarDayRule
{
    public int Order => 10;

    public IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context)
    {
        var attendance = context.Day.Attendance;
        if (attendance is null || !attendance.IsNumericVariance)
            return Array.Empty<CalendarIssueDto>();

        var attendanceItem = context.SourceItems.FirstOrDefault(x => x.ModuleCode == "ATTENDANCE");

        var actions = new List<CalendarActionOptionDto>
        {
            new()
            {
                Code = "OPEN_HR_FEEDBACK",
                Title = "Phản hồi nhân sự",
                Description = "Báo sự cố/chênh lệch chấm công và có thể bổ sung bằng chứng.",
                Kind = "FORM",
                DetailRoute = attendance.DetailRoute ?? attendanceItem?.DetailRoute,
                RequiresReason = true,
                RequiresAttachment = true
            }
        };

        if (attendance.ActualHours.HasValue
            && attendance.RequiredHours.HasValue
            && attendance.ActualHours.Value > attendance.RequiredHours.Value)
        {
            actions.Insert(0, new CalendarActionOptionDto
            {
                Code = "OPEN_OT",
                Title = "Đăng ký OT",
                Description = "Thực tế vượt giờ ca; mở màn hình đăng ký OT cho ngày này.",
                Kind = "NAVIGATION",
                DetailRoute = $"/ot/create?date={context.Day.Date:yyyy-MM-dd}"
            });
        }

        var summary = attendance.ActualHours.HasValue && attendance.RequiredHours.HasValue
            ? $"Thực tế {attendance.ActualHours.Value:0.##} giờ / ca {attendance.RequiredHours.Value:0.##} giờ."
            : $"Giá trị chấm công: {attendance.DisplayValue ?? "chênh lệch"}.";

        return new[]
        {
            new CalendarIssueDto
            {
                Code = "ATTENDANCE_TIME_VARIANCE",
                ModuleCode = "ATTENDANCE",
                Severity = 2,
                Marker = "?",
                Title = "Chênh lệch giờ công",
                Summary = summary,
                SourceId = attendance.SourceId ?? attendanceItem?.SourceId,
                ActionId = attendance.ActionId ?? attendanceItem?.ActionId,
                DetailRoute = attendance.DetailRoute ?? attendanceItem?.DetailRoute,
                Actions = actions
            }
        };
    }
}