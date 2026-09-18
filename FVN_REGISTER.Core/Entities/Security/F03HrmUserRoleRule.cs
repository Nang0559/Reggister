using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03HrmUserRoleRules")]
public sealed class F03HrmUserRoleRule
{
    [Key]
    public int Id { get; set; }

    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public string? LastModifiedSource { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    [StringLength(20)]
    public string? DeptCode { get; set; }

    [StringLength(20)]
    public string? PositionCode { get; set; }

    public int PermissionCode { get; set; }
    public int Priority { get; set; } = 100;

    [StringLength(500)]
    public string? Note { get; set; }
}