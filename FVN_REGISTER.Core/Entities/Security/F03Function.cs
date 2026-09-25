using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Functions")]
public partial class F03Function : BaseAuditEntity
{
    // FunctionCode remains the legacy numeric identity for backward compatibility.
    [Required]
    public int FunctionCode { get; set; }

    [Required, StringLength(150)]
    public string FunctionKey { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FunctionName { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Detail { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ModuleCode { get; set; }

    [StringLength(50)]
    public string? ActionCode { get; set; }

    [StringLength(30)]
    public string? ScopeCode { get; set; }

    [StringLength(30)]
    public string LifecycleStatus { get; set; } = "Active";

    [StringLength(30)]
    public string SourceType { get; set; } = "Legacy";

    public DateTime? LastSeenAt { get; set; }

    [StringLength(150)]
    public string? ReplacementFunctionKey { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<F03UserFunction> UserFunctions { get; set; } = new List<F03UserFunction>();
    public virtual ICollection<F03RoleFunction> RoleFunctions { get; set; } = new List<F03RoleFunction>();
}
