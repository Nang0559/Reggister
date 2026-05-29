using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03workYear
{
    public int Id { get; set; }

    public int? WorkYear { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Remark { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }
}
