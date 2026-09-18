
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03Functions")]
public partial class F03Function : BaseAuditEntity
{

    // Dùng để code gọi (ví dụ: 101, 102...)
    [Required]
    public int FunctionCode { get; set; }

    [Required, StringLength(100)]
    public string FunctionName { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Detail { get; set; } = string.Empty;

    // Navigation (để mapping ngược lại nếu cần kiểm tra xem chức năng này được gán cho ai)
    public virtual ICollection<F03UserFunction> UserFunctions { get; set; } = new List<F03UserFunction>();
}
