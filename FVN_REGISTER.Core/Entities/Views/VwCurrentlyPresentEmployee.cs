using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Core.Entities.Views;

public partial class VwCurrentlyPresentEmployee
{
    public string EmployeeId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public DateOnly? Date { get; set; }

    public DateTime? CheckInTime { get; set; }
}
