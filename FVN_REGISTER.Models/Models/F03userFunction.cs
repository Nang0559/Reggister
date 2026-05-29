using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03userFunction
{
    public int IdUser { get; set; }

    public int IdPermission { get; set; }

    public int IdFunction { get; set; }

    public virtual F03permission IdPermissionNavigation { get; set; } = null!;

    public virtual F03user IdUserNavigation { get; set; } = null!;
}
