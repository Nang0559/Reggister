using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string Action { get; set; } = null!;

    public string? Description { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual F03user? User { get; set; }
}
