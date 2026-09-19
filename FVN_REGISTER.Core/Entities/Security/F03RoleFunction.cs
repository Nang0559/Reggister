using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03RoleFunctions")]
public sealed class F03RoleFunction:BaseAuditEntity
{
    
    public int IdFunction { get; set; }
    public  F03Role Role { get; set; } = null!;
    public  F03Function Function { get; set; } = null!;
}
