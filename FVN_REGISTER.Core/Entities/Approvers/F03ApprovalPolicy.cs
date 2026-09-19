using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Approvers;

[Table("F03ApprovalPolicies")]
public sealed class F03ApprovalPolicy : BaseAuditEntity
{
    [Required]
    public RequestModule RequestType { get; set; }

    /// <summary>
    /// Approval group resolved from the HRM PositionCode.
    /// Null or "*" is the optional default policy.
    /// </summary>
    [StringLength(30)]
    public string? ApprovalGroupCode { get; set; }

    /// <summary>
    /// Kept for backward compatibility during migration. Runtime routing must
    /// resolve PositionCode -> ApprovalGroupCode before reading policies.
    /// </summary>
    [StringLength(20)]
    public string? RequesterPositionCode { get; set; }

    public int Level { get; set; }

    public int Sequence { get; set; }

    [Required, StringLength(100)]
    public string LevelName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool Required { get; set; } = true;
}
