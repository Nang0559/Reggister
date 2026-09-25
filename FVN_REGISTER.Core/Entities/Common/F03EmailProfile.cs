using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EmailProfiles")]
public partial class F03EmailProfile : BaseAuditEntity
{
    public int ParentId { get; set; }
    public bool IsGroup { get; set; }

    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NameEn { get; set; }

    [Required, StringLength(100)]
    public string EmailServerName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string EmailServerType { get; set; } = "SMTP";

    public int EmailServerPort { get; set; } = 587;
    public bool EmailServerEnableSsl { get; set; } = true;

    [StringLength(20)]
    public string SecurityMode { get; set; } = "STARTTLS";

    [StringLength(20)]
    public string AuthenticationType { get; set; } = "Basic";

    [Required, StringLength(100)]
    public string EmailAccountName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string EmailAddress { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FromName { get; set; }

    [StringLength(100)]
    public string? ReplyTo { get; set; }

    [StringLength(255)]
    public string? EmailPassword { get; set; }

    [StringLength(255)]
    public string? SiteUrl { get; set; }

    public bool IsDefault { get; set; }
    public int TimeoutSeconds { get; set; } = 30;

    [Timestamp]
    public byte[] Timestamp { get; set; } = null!;
}
