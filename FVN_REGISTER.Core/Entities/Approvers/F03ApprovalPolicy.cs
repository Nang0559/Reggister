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
    /// Canonical HRM position code of the requester.
    /// This value is the same PositionCode used by F03Employee/F03User
    /// and is resolved directly against F03Positions.
    /// </summary>
    [Required, StringLength(20)]
    public string PositionCode { get; set; } = string.Empty;

    public int Level { get; set; }

    public int Sequence { get; set; }

    [Required, StringLength(100)]
    public string LevelName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool Required { get; set; } = true;
}
