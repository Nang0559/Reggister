using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03department
{
    public int Id { get; set; }

    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }
}
