
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.HR;

[Table("F03Genders")]
public partial class F03Gender : BaseAuditEntity
{
    // Id kế thừa từ BaseAuditEntity

    [Required, StringLength(10)]
    public string GenderCode { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string GenderName { get; set; } = string.Empty;
}
