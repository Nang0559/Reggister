using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EmailDispatchPolicies")]
public partial class F03EmailDispatchPolicy : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string TemplateCode { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string EmailProfileCode { get; set; } = string.Empty;

    public EmailDispatchMode DispatchMode { get; set; } = EmailDispatchMode.AutoSend;

    public int Priority { get; set; } = 100;

    [StringLength(500)]
    public string? Description { get; set; }
}
