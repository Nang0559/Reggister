using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VF03leaveDay
{
    public int? Id { get; set; }

    public int? WorkYear { get; set; }

    public string? EmployeeCode { get; set; }

    public decimal? PhepTon { get; set; }

    public decimal? TongPhep { get; set; }

    public string EmployeeName { get; set; } = null!;

    public string? GenderName { get; set; }

    public string? DeptName { get; set; }

    public string EmailAddress { get; set; } = null!;

    public DateTime? RegisterDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? LeaveDay { get; set; }

    public decimal? TotalDay { get; set; }

    public decimal? TotalLeaveDay { get; set; }

    public string? LeaveReason { get; set; }

    public string? RequestStatus { get; set; }

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

    public bool? Sync { get; set; }

    public DateTime? LastSync { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedAtText { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string Cvcode { get; set; } = null!;

    public string Cvname { get; set; } = null!;

    public string? DeptCode { get; set; }

    public string? Level2ApproveCode { get; set; }

    public string? Level1ApproveCode { get; set; }

    public bool? Level3IsApprove { get; set; }

    public string? Level3ApproveName { get; set; }

    public string? Level3ApproveEmail { get; set; }

    public DateTime? Level3ApproveTime { get; set; }

    public string? Level3Comment { get; set; }

    public string? Level3ApproveCode { get; set; }

    public string? AttachedDocuments { get; set; }
}
