using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03emailProfile
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public bool IsGroup { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Name2 { get; set; } = null!;

    public string EmailServerName { get; set; } = null!;

    public string EmailServerType { get; set; } = null!;

    public int EmailServerPort { get; set; }

    public bool EmailServerEnableSsl { get; set; }

    public string EmailAccountName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string? EmailPassword { get; set; }

    public string? SiteUrl { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public byte[] Timestamp { get; set; } = null!;
}
