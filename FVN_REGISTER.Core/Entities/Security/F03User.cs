using FVN_REGISTER.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Users")]
public partial class F03User : BaseAuditEntity
{
    // PK kế thừa từ BaseAuditEntity.Id.
    // EF mapping Id -> IdUser được đặt tại Infrastructure để giữ nguyên schema DB legacy.

    [Required, StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Avatar { get; set; }

    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    public int PermissionCode { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool LockoutEnable { get; set; } = true;
    public DateTime? LockoutEndDate { get; set; }
    public int NumLoginFailed { get; set; }

    public int LevelApprove { get; set; }

    [StringLength(20)]
    public string? DeptCode { get; set; }

    [StringLength(20)]
    public string? Cvcode { get; set; }

    [ForeignKey(nameof(PermissionCode))]
    public virtual F03Permission PermissionCodeNavigation { get; set; } = null!;

    public virtual ICollection<F03AuditLog> AuditLogs { get; set; } = new List<F03AuditLog>();
    public virtual ICollection<F03UserFunction> F03userFunctions { get; set; } = new List<F03UserFunction>();
}
