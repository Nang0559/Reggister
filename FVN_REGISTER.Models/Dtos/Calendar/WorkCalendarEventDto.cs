namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class WorkCalendarEventDto
{
    public string Id { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public int? RequestId { get; set; }
    public string? SourceId { get; set; }
    public string? SubTypeCode { get; set; }
    public bool IsHalfDay { get; set; }
    public decimal DayValue { get; set; } = 1m;
    public Guid? ActionId { get; set; }
    public string? DetailRoute { get; set; }

    // Approval snapshot shown directly on the work calendar.
    public int? ApprovalLevel { get; set; }
    public string? ApprovalLevelName { get; set; }
    public string? CurrentApproverName { get; set; }
    public string? ApprovalStatus { get; set; }
    public bool IsApproved { get; set; }
}