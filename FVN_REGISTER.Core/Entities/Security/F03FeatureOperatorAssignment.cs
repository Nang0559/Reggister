using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03FeatureOperatorAssignments")]
public sealed class F03FeatureOperatorAssignment : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    public int FunctionCode { get; set; }

    [Required, StringLength(50)]
    public string ResourceType { get; set; } = string.Empty;

    public int? ResourceId { get; set; }

    [StringLength(500)]
    public string? Remark { get; set; }
}
