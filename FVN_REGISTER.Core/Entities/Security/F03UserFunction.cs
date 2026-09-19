using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03UserFunctions")]
public partial class F03UserFunction : BaseAuditEntity
{
    public int IdUser { get; set; }
    public int IdPermission { get; set; }
    public int IdFunction { get; set; }

    [ForeignKey(nameof(IdUser))]
    public virtual F03User User { get; set; } = null!;

    [ForeignKey(nameof(IdPermission))]
    public virtual F03Permission Permission { get; set; } = null!;

    [ForeignKey(nameof(IdFunction))]
    public virtual F03Function Function { get; set; } = null!;
}