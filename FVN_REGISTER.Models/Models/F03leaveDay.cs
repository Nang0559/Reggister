using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03leaveDay
{
    public int Id { get; set; }

    public int WorkYear { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public DateTime RegisterDate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal TotalDay { get; set; }

    public int RegisterId { get; set; }

    public string LeaveReason { get; set; } = null!;

    public string RequestStatus { get; set; } = null!;

    public bool? Level1IsApprove { get; set; }

    public string? Level1ApproveName { get; set; }

    public string? Level1ApproveEmail { get; set; }

    public DateTime? Level1ApproveTime { get; set; }

    public string? Level1Comment { get; set; }

    public bool? Level2IsApprove { get; set; }

    public string? Level2ApproveName { get; set; }

    public string? Level2ApproveEmail { get; set; }

    public DateTime? Level2ApproveTime { get; set; }

    public string? Level2Comment { get; set; }

    public bool? Sync { get; set; }

    public DateTime? LastSync { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Level1ApproveCode { get; set; }

    public string? Level2ApproveCode { get; set; }

    public bool? Level3IsApprove { get; set; }

    public string? Level3ApproveName { get; set; }

    public string? Level3ApproveEmail { get; set; }

    public DateTime? Level3ApproveTime { get; set; }

    public string? Level3Comment { get; set; }

    public string? Level3ApproveCode { get; set; }

    public string? LeaveTypeCode { get; set; }

    public decimal? TotalLeaveDay { get; set; }

    public bool? NotifiedLv1 { get; set; }

    public bool? NotifiedLv2 { get; set; }

    public bool? NotifiedLv3 { get; set; }

    public DateTime? NotifiedLv1At { get; set; }

    public DateTime? NotifiedLv2At { get; set; }

    public DateTime? NotifiedLv3At { get; set; }

    public virtual ICollection<F03leaveDayDetail> F03leaveDayDetails { get; set; } = new List<F03leaveDayDetail>();
}
