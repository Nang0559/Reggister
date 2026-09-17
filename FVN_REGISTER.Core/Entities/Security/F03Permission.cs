
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Permissions")]
public partial class F03Permission : BaseAuditEntity
{
    [Key]
    public int IdPermission { get; set; }

    // Dùng làm khóa nghiệp vụ (VD: 1: ADMIN, 2: MANAGER, 3: USER)
    public int PermissionCode { get; set; }

    [Required, StringLength(100)]
    public string PermissionName { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Detail { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<F03UserFunction> F03userFunctions { get; set; } = new List<F03UserFunction>();
    public virtual ICollection<F03User> F03users { get; set; } = new List<F03User>();
}
