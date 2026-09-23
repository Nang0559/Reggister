using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class CalendarDayRuleEngine : ICalendarDayRuleEngine
{
    private readonly IReadOnlyList<ICalendarDayRule> _rules;

    public CalendarDayRuleEngine(IEnumerable<ICalendarDayRule> rules)
    {
        _rules = rules.OrderBy(x => x.Order).ToArray();
    }

    public IReadOnlyList<CalendarIssueDto> Evaluate(CalendarDayRuleContext context)
    {
        var result = new List<CalendarIssueDto>();

        foreach (var rule in _rules)
            result.AddRange(rule.Evaluate(context));

        return result
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(
                x => $"{x.Code}|{x.SourceId ?? context.Day.Date.ToString("yyyy-MM-dd")}|{x.RequestId?.ToString() ?? ""}",
                StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.Code)
            .ToArray();
    }
}