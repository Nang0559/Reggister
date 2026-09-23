using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface ICalendarDayRule
{
    int Order { get; }
    IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context);
}