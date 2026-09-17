using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Core.Entities.Views;

public partial class vF03EmployeeAttendance
{
    public string EmployeeId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int Department { get; set; }

    public DateOnly? Date { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }
}
