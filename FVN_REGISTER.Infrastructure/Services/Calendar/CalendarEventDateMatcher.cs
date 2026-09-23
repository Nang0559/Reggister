using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public static class CalendarEventDateMatcher
{
    /// <summary>
    /// Tests whether a calendar event intersects the requested calendar date.
    /// Events use half-open intervals [Start, End), so an event ending exactly
    /// at midnight does not belong to the following date.
    /// </summary>
    public static bool CoversDate(WorkCalendarEventDto calendarEvent, DateTime date)
    {
        var dayStart = date.Date;
        return calendarEvent.Start.Date <= dayStart
            && calendarEvent.End > dayStart;
    }
}
