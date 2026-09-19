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
    /// Mã chức vụ của người đăng ký. Null hoặc "*" là policy mặc định.
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
