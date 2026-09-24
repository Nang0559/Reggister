using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar.Rules;

public sealed class OtActualWithoutRequestRule : ICalendarDayRule
{
    public int Order => 50;

    public IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context)
    {
        if (context.Day.Attendance?.HasActualOt != true)
            return Array.Empty<CalendarIssueDto>();

        if (context.Day.Registrations.Any(x =>
                x.ModuleCode == "OT" && IsApproved(x.Status)))
            return Array.Empty<CalendarIssueDto>();

        var source = context.SourceItems.FirstOrDefault(x => x.ModuleCode == "OT" && x.RequiresAction);
        long? reconciliationId = long.TryParse(source?.SourceId, out var parsedReconciliationId)
            ? parsedReconciliationId
            : null;

        return new[]
        {
            new CalendarIssueDto
            {
                Code = "OT_ACTUAL_WITHOUT_REQUEST",
                ModuleCode = "OT",
                Severity = 3,
                Marker = "?",
                Title = "Có OT thực tế nhưng chưa có đơn OT",
                Summary = $"Phát hiện {context.Day.Attendance.ActualOtMinutes} phút OT thực tế nhưng chưa có đăng ký OT được duyệt.",
                SourceId = source?.SourceId ?? context.Day.Attendance.SourceId,
                ActionId = source?.ActionId,
                DetailRoute = source?.DetailRoute,
                Actions = new[]
                {
                    new CalendarActionOptionDto
                    {
                        Code = "OPEN_HR_FEEDBACK",
                        Title = "Phản hồi nhân sự",
                        Description = "Báo cho nhân sự về OT thực tế chưa có đăng ký.",
                        Kind = "FORM",
                        DetailRoute = source?.DetailRoute,
                        ReconciliationId = reconciliationId,
                        RequiresReason = true,
                        RequiresAttachment = true
                    },
                    new CalendarActionOptionDto
                    {
                        Code = "OPEN_OT",
                        Title = "Đăng ký OT",
                        Description = "Mở đăng ký OT cho ngày này.",
                        Kind = "NAVIGATION",
                        DetailRoute = $"/ot/create?date={context.Day.Date:yyyy-MM-dd}"
                    }
                }
            }
        };
    }

    private static bool IsApproved(string? status) =>
        string.Equals(status?.Trim(), "Approved", StringComparison.OrdinalIgnoreCase);
}