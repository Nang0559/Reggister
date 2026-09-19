using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Permissions")]
public partial class F03Permission : BaseAuditEntity
{
    // PK duy nhất kế thừa trực tiếp từ BaseAuditEntity.Id.

    public int PermissionCode { get; set; }

    [Required, StringLength(100)]
    public string PermissionName { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Detail { get; set; } = string.Empty;

    public virtual ICollection<F03UserFunction> F03userFunctions { get; set; } = new List<F03UserFunction>();
    public virtual ICollection<F03User> F03users { get; set; } = new List<F03User>();
}
