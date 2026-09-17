
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EmailTemplates")]
public partial class F03EmailTemplate : BaseAuditEntity
{
    // Id đã được kế thừa từ BaseAuditEntity

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty; // Ví dụ: "OT_APPROVE", "LEAVE_NOTIFY"

    [Required, StringLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty; // Nội dung HTML của email

    [StringLength(500)]
    public string? Description { get; set; } // Mô tả mục đích của mẫu email
}
