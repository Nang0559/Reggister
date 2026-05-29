using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class EscalationLog
{
    public int Id { get; set; }

    public int? LeaveId { get; set; }

    public int? Level { get; set; }

    public string? Action { get; set; }

    public DateTime? CreatedAt { get; set; }
}
