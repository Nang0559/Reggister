using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Application.Models.Calendar;

public sealed class CalendarDayRuleContext
{
    public CalendarDayRuleContext(CalendarDayDto day, IReadOnlyList<CalendarItemDto> sourceItems)
    {
        Day = day;
        SourceItems = sourceItems;
    }

    public CalendarDayDto Day { get; }
    public IReadOnlyList<CalendarItemDto> SourceItems { get; }
}