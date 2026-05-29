using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03permission
{
    public int IdPermission { get; set; }

    public int PermissionCode { get; set; }

    public string PermissionName { get; set; } = null!;

    public DateTime? Timestamps { get; set; }

    public string Detail { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<F03userFunction> F03userFunctions { get; set; } = new List<F03userFunction>();

    public virtual ICollection<F03user> F03users { get; set; } = new List<F03user>();
}
