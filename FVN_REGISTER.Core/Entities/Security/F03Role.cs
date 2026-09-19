using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Roles")]
public sealed class F03Role : BaseAuditEntity
{
    [Required]
    public int RoleCode { get; set; }

    [Required, StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Detail { get; set; }

    public bool IsSystem { get; set; }

    public ICollection<F03RoleFunction> RoleFunctions { get; set; } = new List<F03RoleFunction>();
    public ICollection<F03UserRole> UserRoles { get; set; } = new List<F03UserRole>();
}