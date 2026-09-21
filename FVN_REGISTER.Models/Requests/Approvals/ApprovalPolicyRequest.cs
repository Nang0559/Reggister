using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Approvals;

public sealed class ApprovalPolicyRequest
{
    [Required]
    public int RequestType { get; set; }

    [Required, StringLength(20)]
    public string DeptCode { get; set; } = string.Empty;

    /// <summary>Optional requester PositionCode. Null/empty means all positions in the department.</summary>
    [StringLength(20)]
    public string? PositionCode { get; set; }

    /// <summary>Position of the approver selected from F03Positions.</summary>
    [Required, StringLength(20)]
    public string ApprovalPositionCode { get; set; } = string.Empty;

    [Range(1, 7)]
    public int Level { get; set; }

    [Range(1, 100)]
    public int Sequence { get; set; }

    [Required, StringLength(100)]
    public string LevelName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool Required { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
