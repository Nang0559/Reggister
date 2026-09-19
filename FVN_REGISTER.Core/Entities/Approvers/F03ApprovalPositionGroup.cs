using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Approvers;

/// <summary>
/// Local approval classification for an HRM PositionCode.
/// PositionCode remains the HRM source-of-truth; ApprovalGroupCode is only
/// the local classification used to select the approval hierarchy.
/// </summary>
[Table("F03ApprovalPositionGroups")]
public sealed class F03ApprovalPositionGroup : BaseAuditEntity
{
    [Required, StringLength(20)]
    public string PositionCode { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string ApprovalGroupCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ApprovalGroupName { get; set; } = string.Empty;
}
