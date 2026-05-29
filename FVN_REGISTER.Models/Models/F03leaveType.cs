using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03leaveType
{
    public int LeaveTypeId { get; set; }

    public string LeaveTypeCode { get; set; } = null!;

    public string LeaveTypeName { get; set; } = null!;

    public string? LeaveTypeName2 { get; set; }

    public bool? TinhPhep { get; set; }

    public string? Hrmcode { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }
}
