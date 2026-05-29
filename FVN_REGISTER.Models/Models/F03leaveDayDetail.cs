using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03leaveDayDetail
{
    public int Id { get; set; }

    public int LeaveDaysId { get; set; }

    public DateOnly LeaveDate { get; set; }

    public string LeaveTypeCode { get; set; } = null!;

    public string? LeaveTypeName { get; set; }

    public bool TinhPhep { get; set; }

    public bool IsHalfDay { get; set; }

    public string? HalfDayOption { get; set; }

    public decimal DayValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual F03leaveDay LeaveDays { get; set; } = null!;
}
