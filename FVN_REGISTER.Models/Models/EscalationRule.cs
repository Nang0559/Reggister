using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class EscalationRule
{
    public int Id { get; set; }

    public int? Level { get; set; }

    public int? TimeoutDays { get; set; }

    public string? DeptCode { get; set; }

    public string? LeaveTypeCode { get; set; }

    public bool? IsActive { get; set; }
}
