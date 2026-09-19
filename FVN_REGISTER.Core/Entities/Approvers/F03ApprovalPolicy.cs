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
    /// Canonical approval classification resolved from the requester's HRM PositionCode.
    /// Runtime routing must resolve:
    /// F03Employee.PositionCode -> F03ApprovalPositionGroup.ApprovalGroupCode
    /// before reading this policy.
    /// </summary>
    [Required, StringLength(30)]
    public string ApprovalGroupCode { get; set; } = string.Empty;

    /// <summary>
    /// Legacy migration column only. Runtime approval routing must not use it.
    /// It can be removed after all deployed databases have migrated to ApprovalGroupCode.
    /// </summary>
    [Obsolete("Use ApprovalGroupCode. Runtime routing must resolve PositionCode through F03ApprovalPositionGroup.")]
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
