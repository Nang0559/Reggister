using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VF03leaveDayDetail
{
    public long? RowId { get; set; }

    public int LeaveId { get; set; }

    public int WorkYear { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string? EmployeeName { get; set; }

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public string? EmailAddress { get; set; }

    public int DetailId { get; set; }

    public DateOnly LeaveDate { get; set; }

    public string LeaveTypeCode { get; set; } = null!;

    public string? LeaveTypeName { get; set; }

    public bool TinhPhep { get; set; }

    public bool IsHalfDay { get; set; }

    public string? HalfDayOption { get; set; }

    public decimal DayValue { get; set; }

    public string? LeaveDateText { get; set; }

    public string RequestStatus { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? Level1IsApprove { get; set; }

    public string? Level1ApproveName { get; set; }

    public bool? Level2IsApprove { get; set; }

    public string? Level2ApproveName { get; set; }

    public bool? Level3IsApprove { get; set; }

    public string? Level3ApproveName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }
}
