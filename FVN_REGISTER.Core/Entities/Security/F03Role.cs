using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Roles")]
public sealed class F03Role
{
    [Key]
    public int IdRole { get; set; }
    public int RoleCode { get; set; }
    [Required, StringLength(100)]
    public string RoleName { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Detail { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public virtual ICollection<F03RoleFunction> RoleFunctions { get; set; } = new List<F03RoleFunction>();
    public virtual ICollection<F03UserRole> UserRoles { get; set; } = new List<F03UserRole>();
}
