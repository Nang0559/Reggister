
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
    public string? NameEn { get; set; } // Thay cho Name2

    [Required, StringLength(100)]
    public string EmailServerName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string EmailServerType { get; set; } = string.Empty; // Smtp, Imap...

    public int EmailServerPort { get; set; }

    public bool EmailServerEnableSsl { get; set; }

    [Required, StringLength(100)]
    public string EmailAccountName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string EmailAddress { get; set; } = string.Empty;

    [StringLength(255)]
    public string? EmailPassword { get; set; } // Nên mã hóa trước khi lưu

    [StringLength(255)]
    public string? SiteUrl { get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; } = null!;
}
