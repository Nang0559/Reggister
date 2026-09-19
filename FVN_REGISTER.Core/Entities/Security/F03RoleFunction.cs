using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03RoleFunctions")]
public sealed class F03RoleFunction : BaseAuditEntity
{
    public int IdRole { get; set; }
    public int IdFunction { get; set; }

    [ForeignKey(nameof(IdRole))]
    public F03Role Role { get; set; } = null!;

    [ForeignKey(nameof(IdFunction))]
    public F03Function Function { get; set; } = null!;
}