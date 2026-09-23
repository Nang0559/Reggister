using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Infrastructure.Services.Calendar;
using FVN_REGISTER.Infrastructure.Services.Calendar.Rules;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class CalendarDayRuleTests
{
    [Fact]
    public void NumericAttendanceVariance_ProducesWarningAndOtAction_WhenActualExceedsShift()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 24),
            IsFuture = true,
            IsPast = false,
            Attendance = new CalendarAttendanceInfoDto
            {
                ActualHours = 10m,
                RequiredHours = 8m,
                WorkMinutes = 600,
                RequiredMinutes = 480,
                IsNumericVariance = true,
                DisplayValue = "10"
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new AttendanceNumericVarianceRule()
        });

        var issues = engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>()));

        var issue = Assert.Single(issues);
        Assert.Equal("ATTENDANCE_TIME_VARIANCE", issue.Code);
        Assert.Equal("ATTENDANCE", issue.ModuleCode);
        Assert.Equal((byte)2, issue.Severity);
        Assert.Contains(issue.Actions, x => x.Code == "OPEN_OT");
        Assert.Contains(issue.Actions, x => x.Code == "OPEN_HR_FEEDBACK");
    }

    [Fact]
    public void OtNoActual_OffersFeedbackAndCancel_WhenPastDayHasApprovedOtWithoutActual()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 22),
            IsPast = true,
            Attendance = new CalendarAttendanceInfoDto
            {
                HasActual = true,
                HasActualOt = false
            },
            Registrations = new[]
            {
                new CalendarRegistrationDto
                {
                    ModuleCode = "OT",
                    RequestId = 123,
                    Status = "Approved",
                    Title = "OT 18:00-20:00",
                    Start = new DateTime(2026, 9, 22, 18, 0, 0),
                    End = new DateTime(2026, 9, 22, 20, 0, 0)
                }
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new OtNoActualRule()
        });

        var issues = engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>()));

        var issue = Assert.Single(issues);
        Assert.Equal("OT_NO_ACTUAL", issue.Code);
        Assert.Equal("OT", issue.ModuleCode);
        Assert.Equal((byte)3, issue.Severity);
        Assert.Contains(issue.Actions, x => x.Code == "OPEN_HR_FEEDBACK");
        Assert.Contains(issue.Actions, x => x.Code == "CANCEL_OT" && x.RequestId == 123);
    }

    [Fact]
    public void OtNoActual_DoesNotTrigger_WhenOtIsPending()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 22),
            IsPast = true,
            Registrations = new[]
            {
                new CalendarRegistrationDto
                {
                    ModuleCode = "OT",
                    RequestId = 123,
                    Status = "Pending",
                    Start = new DateTime(2026, 9, 22, 18, 0, 0),
                    End = new DateTime(2026, 9, 22, 20, 0, 0)
                }
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new OtNoActualRule()
        });

        Assert.Empty(engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>())));
    }

    [Fact]
    public void ActualOtWithoutRequest_Triggers_WhenOnlyUnapprovedOtExists()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 21),
            Attendance = new CalendarAttendanceInfoDto
            {
                HasActualOt = true,
                ActualOtMinutes = 90
            },
            Registrations = new[]
            {
                new CalendarRegistrationDto
                {
                    ModuleCode = "OT",
                    RequestId = 55,
                    Status = "Pending"
                }
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new OtActualWithoutRequestRule()
        });

        var issue = Assert.Single(engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>())));

        Assert.Equal("OT_ACTUAL_WITHOUT_REQUEST", issue.Code);
    }

    [Fact]
    public void LeaveConflict_ProducesCriticalIssue_ForFullDayP()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 21),
            Attendance = new CalendarAttendanceInfoDto { HasActual = true },
            Registrations = new[]
            {
                new CalendarRegistrationDto
                {
                    ModuleCode = "LEAVE",
                    Status = "Approved",
                    SubTypeCode = "P",
                    IsHalfDay = false,
                    DayValue = 1m,
                    RequestId = 9,
                    Title = "Phép năm"
                }
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new LeaveAttendanceConflictRule()
        });

        var issues = engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>()));

        var issue = Assert.Single(issues);
        Assert.Equal("LEAVE_HAS_ATTENDANCE", issue.Code);
        Assert.Equal((byte)3, issue.Severity);
    }

    [Fact]
    public void HalfDayLeave_DoesNotProduceFullDayConflict()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 21),
            Attendance = new CalendarAttendanceInfoDto { HasActual = true },
            Registrations = new[]
            {
                new CalendarRegistrationDto
                {
                    ModuleCode = "LEAVE",
                    SubTypeCode = "P",
                    IsHalfDay = true,
                    DayValue = 0.5m
                }
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new LeaveAttendanceConflictRule()
        });

        var issues = engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>()));

        Assert.Empty(issues);
    }

    [Fact]
    public void ActualOtWithoutRequest_ProducesCriticalIssue()
    {
        var day = new CalendarDayDto
        {
            Date = new DateOnly(2026, 9, 21),
            Attendance = new CalendarAttendanceInfoDto
            {
                HasActualOt = true,
                ActualOtMinutes = 90
            }
        };

        var engine = new CalendarDayRuleEngine(new ICalendarDayRule[]
        {
            new OtActualWithoutRequestRule()
        });

        var issues = engine.Evaluate(new CalendarDayRuleContext(
            day,
            Array.Empty<CalendarItemDto>()));

        var issue = Assert.Single(issues);
        Assert.Equal("OT_ACTUAL_WITHOUT_REQUEST", issue.Code);
        Assert.Equal((byte)3, issue.Severity);
    }
}