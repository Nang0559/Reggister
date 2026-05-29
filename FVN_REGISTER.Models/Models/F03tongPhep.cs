using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03tongPhep
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public int WorkYear { get; set; }

    public decimal TongPhep { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }
}
