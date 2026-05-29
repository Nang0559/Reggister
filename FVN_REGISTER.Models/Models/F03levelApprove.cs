using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03levelApprove
{
    public int Id { get; set; }

    public int? LevelId { get; set; }

    public string? LevelName { get; set; }
}
