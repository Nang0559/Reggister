using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Core.Entities.PublicForms;

[Table("F03PublicFormAudiences")]
public sealed class F03PublicFormAudience : BaseAuditEntity
{
    public int FormId { get; set; }
    [Required, StringLength(20)] public string ScopeType { get; set; } = "AllCompany";
    [StringLength(100)] public string? ScopeValue { get; set; }
}