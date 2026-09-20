using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.OT;

public sealed class OTLimitScopePreviewDto
{
    public OTLimitScopeType ScopeType { get; set; }
    public string ScopeCode { get; set; } = string.Empty;
    public string ScopeName { get; set; } = string.Empty;

    public decimal RequestedHours { get; set; }

    public decimal UsedHoursThisWeek { get; set; }
    public decimal? WeeklyLimit { get; set; }
    public decimal? ProjectedHoursThisWeek => WeeklyLimit.HasValue ? UsedHoursThisWeek + RequestedHours : null;

    public decimal UsedHoursThisMonth { get; set; }
    public decimal? MonthlyLimit { get; set; }
    public decimal? ProjectedHoursThisMonth => MonthlyLimit.HasValue ? UsedHoursThisMonth + RequestedHours : null;

    public decimal UsedHoursThisYear { get; set; }
    public decimal? YearlyLimit { get; set; }
    public decimal? ProjectedHoursThisYear => YearlyLimit.HasValue ? UsedHoursThisYear + RequestedHours : null;

    public bool ExceedsWeekly => WeeklyLimit.HasValue && ProjectedHoursThisWeek > WeeklyLimit.Value;
    public bool ExceedsMonthly => MonthlyLimit.HasValue && ProjectedHoursThisMonth > MonthlyLimit.Value;
    public bool ExceedsYearly => YearlyLimit.HasValue && ProjectedHoursThisYear > YearlyLimit.Value;
    public bool IsExceeded => ExceedsWeekly || ExceedsMonthly || ExceedsYearly;
}
