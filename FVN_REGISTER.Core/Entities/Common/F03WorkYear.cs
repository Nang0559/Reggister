using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03WorkYears")]
public partial class F03WorkYear : BaseAuditEntity
{
    [Required]
    public int WorkYear { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(500)]
    public string? Remark { get; set; }
}
