using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03RoleFunctions")]
public sealed class F03RoleFunction
{
    public int IdRole { get; set; }
    public int IdFunction { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public virtual F03Role Role { get; set; } = null!;
    public virtual F03Function Function { get; set; } = null!;
}
