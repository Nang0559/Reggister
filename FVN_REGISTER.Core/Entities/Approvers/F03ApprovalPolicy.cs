using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Approvers;

[Table("F03ApprovalPolicies")]
public sealed class F03ApprovalPolicy : BaseAuditEntity
{
    [Required]
    public RequestModule RequestType { get; set; }

    /// <summary>Mandatory requester department scope.</summary>
    [Required, StringLength(20)]
    public string DeptCode { get; set; } = string.Empty;

    /// <summary>Optional requester position scope. Empty means all positions in DeptCode.</summary>
    [StringLength(20)]
    public string? PositionCode { get; set; }

    /// <summary>HRM position selected as the approval level/approver role.</summary>
    [Required, StringLength(20)]
    public string ApprovalPositionCode { get; set; } = string.Empty;

    public int Level { get; set; }

    public int Sequence { get; set; }

    [Required, StringLength(100)]
    public string LevelName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool Required { get; set; } = true;
}
