using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Functions")]
public partial class F03Function : BaseAuditEntity
{
    // PK kế thừa từ BaseAuditEntity.Id.
    // EF mapping Id -> IdFunction được đặt tại Infrastructure.

    [Required]
    public int FunctionCode { get; set; }

    [Required, StringLength(100)]
    public string FunctionName { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Detail { get; set; } = string.Empty;

    public virtual ICollection<F03UserFunction> UserFunctions { get; set; } = new List<F03UserFunction>();
}
