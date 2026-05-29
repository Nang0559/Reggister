using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03userLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string LastSeen { get; set; } = null!;

    public string LastSeenUrl { get; set; } = null!;

    public string ApplicationName { get; set; } = null!;

    public string ApplicationVerion { get; set; } = null!;

    public string WorkstationName { get; set; } = null!;

    public string WorkstationUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
