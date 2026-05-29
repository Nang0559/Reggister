using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VF03leaveDays1
{
    public int Id { get; set; }

    public int WorkYear { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string? EmployeeName { get; set; }

    public string? GenderName { get; set; }

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public string? EmailAddress { get; set; }

    public DateTime RegisterDate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? LeaveDayText { get; set; }

    public bool? IsHalfDay { get; set; }

    public string? HalfDayOption { get; set; }

    public string LeaveDurationText { get; set; } = null!;

    public decimal TotalLeaveDay { get; set; }

    public bool? IsPaidLeave { get; set; }

    public decimal TotalPaidLeaveDay { get; set; }

    public decimal TotalUnpaidLeaveDay { get; set; }

    public string? LeaveTypeCode { get; set; }

    public string? LeaveTypeName { get; set; }

    public string? Hrmcode { get; set; }

    public string LeaveReason { get; set; } = null!;

    public string RequestStatus { get; set; } = null!;

    public bool? Level1IsApprove { get; set; }

    public string Level1IsApproveText { get; set; } = null!;

    public string? Level1ApproveName { get; set; }

    public string? Level1ApproveEmail { get; set; }

    public DateTime? Level1ApproveTime { get; set; }

    public string? Level1ApproveTimeText { get; set; }

    public string? Level1Comment { get; set; }

    public bool? Level2IsApprove { get; set; }

    public string Level2IsApproveText { get; set; } = null!;

    public string? Level2ApproveName { get; set; }

    public string? Level2ApproveEmail { get; set; }

    public DateTime? Level2ApproveTime { get; set; }

    public string? Level2ApproveTimeText { get; set; }

    public string? Level2Comment { get; set; }

    public bool? Level3IsApprove { get; set; }

    public string? Level3ApproveName { get; set; }

    public string? Level3ApproveEmail { get; set; }

    public DateTime? Level3ApproveTime { get; set; }

    public string? Level3Comment { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedAtText { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Cvcode { get; set; }

    public string? Cvname { get; set; }

    public string? AttachedDocuments { get; set; }
}
