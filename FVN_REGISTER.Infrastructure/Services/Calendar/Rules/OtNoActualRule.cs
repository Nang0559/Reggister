using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar.Rules;

public sealed class OtNoActualRule : ICalendarDayRule
{
    public int Order => 20;

    public IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context)
    {
        if (!context.Day.IsPast || context.Day.Attendance?.HasActualOt == true)
            return Array.Empty<CalendarIssueDto>();

        var ot = context.Day.Registrations.FirstOrDefault(x =>
            x.ModuleCode == "OT" && IsApproved(x.Status));
        if (ot is null)
            return Array.Empty<CalendarIssueDto>();

        var source = context.SourceItems.FirstOrDefault(x => x.ModuleCode == "OT" && (x.RequiresAction || x.DetailRoute != null));

        return new[]
        {
            new CalendarIssueDto
            {
                Code = "OT_NO_ACTUAL",
                ModuleCode = "OT",
                Severity = 3,
                Marker = "?",
                Title = "OT chưa có giờ thực tế",
                Summary = $"Đã có đăng ký OT {FormatTime(ot)} nhưng chưa có OT thực tế.",
                SourceId = ot.RequestId?.ToString() ?? ot.SourceIdFallback(),
                ActionId = source?.ActionId,
                DetailRoute = source?.DetailRoute,
                RequestId = ot.RequestId,
                Actions = new[]
                {
                    new CalendarActionOptionDto
                    {
                        Code = "OPEN_HR_FEEDBACK",
                        Title = "Phản hồi sự cố chấm công",
                        Description = "Báo cho nhân sự về việc OT không có giờ chấm công thực tế.",
                        Kind = "FORM",
                        DetailRoute = source?.DetailRoute,
                        RequiresReason = true,
                        RequiresAttachment = true
                    },
                    new CalendarActionOptionDto
                    {
                        Code = "CANCEL_OT",
                        Title = "Tôi xin hủy OT",
                        Description = "Hủy chính đăng ký OT này và ghi nhận lý do do người dùng hủy.",
                        Kind = "COMMAND",
                        RequestId = ot.RequestId,
                        RequiresReason = true
                    }
                }
            }
        };
    }

    private static bool IsApproved(string? status) =>
        string.Equals(status?.Trim(), "Approved", StringComparison.OrdinalIgnoreCase);

    private static string FormatTime(CalendarRegistrationDto registration)
    {
        return registration.Start.Date == registration.End.Date
            ? $"{registration.Start:HH:mm}-{registration.End:HH:mm}"
            : $"{registration.Start:dd/MM HH:mm}-{registration.End:dd/MM HH:mm}";
    }
}

file static class CalendarRegistrationDtoExtensions
{
    public static string SourceIdFallback(this CalendarRegistrationDto registration) =>
        $"{registration.ModuleCode}:{registration.Start:yyyyMMdd}";
}