using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03PublicInformation")]
public sealed class F03PublicInformation : BaseAuditEntity
{
    [Required, StringLength(30)] public string Type { get; set; } = "Announcement";
    [Required, StringLength(300)] public string Title { get; set; } = string.Empty;
    [StringLength(1000)] public string? Summary { get; set; }
    public string? Content { get; set; }
    [Required, StringLength(30)] public string Status { get; set; } = "Draft";
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsImportant { get; set; }
    [StringLength(1000)] public string? AttachmentUrl { get; set; }
    public int? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
}
