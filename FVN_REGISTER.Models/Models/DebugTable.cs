using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class DebugTable
{
    public string? EmployeeCode { get; set; }

    public string? Nvma { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? LeaveTypeCode { get; set; }

    public string? LeaveReason { get; set; }

    public string? LastDknma { get; set; }

    public string? NewDknma { get; set; }
}
