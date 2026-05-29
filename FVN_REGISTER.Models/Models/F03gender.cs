using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03gender
{
    public int Id { get; set; }

    public string GenderCode { get; set; } = null!;

    public string GenderName { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }
}
