using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Core.Entities.Views;

public partial class VF03leaveType
{
    public int LeaveTypeId { get; set; }

    public string LeaveTypeCode { get; set; } = null!;

    public string LeaveTypeName { get; set; } = null!;

    public string? LeaveTypeName2 { get; set; }

    public bool? IsCountedAsLeave { get; set; }

    public string CountedAsLeaveName { get; set; } = null!;

    public string? Hrmcode { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }
}
