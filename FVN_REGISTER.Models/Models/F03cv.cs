using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03cv
{
    public int Id { get; set; }

    public string Cvcode { get; set; } = null!;

    public string Cvname { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public bool? IsApprove { get; set; }

    public bool? IsAllowApprove { get; set; }
}
