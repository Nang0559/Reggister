using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03function
{
    public int IdFunction { get; set; }

    public int FunctionCode { get; set; }

    public string FunctionName { get; set; } = null!;

    public DateTime? Timestamps { get; set; }

    public string Detail { get; set; } = null!;

    public bool? IsActive { get; set; }
}
