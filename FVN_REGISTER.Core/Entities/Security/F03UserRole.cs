using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03UserRoles")]
public sealed class F03UserRole : BaseAuditEntity
{
    public int IdUser { get; set; }
    public int IdRole { get; set; }
    public bool IsPrimary { get; set; }

    public F03User User { get; set; } = null!;
    public F03Role Role { get; set; } = null!;
}