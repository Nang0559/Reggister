using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03UserRoles")]
public sealed class F03UserRole
{
    public int IdUser { get; set; }
    public int IdRole { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public virtual F03User User { get; set; } = null!;
    public virtual F03Role Role { get; set; } = null!;
}
