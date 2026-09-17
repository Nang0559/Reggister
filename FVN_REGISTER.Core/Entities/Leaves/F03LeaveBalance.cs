
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Leaves;

[Table("F03LeaveBalances")]
public partial class F03LeaveBalance : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    public int WorkYear { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalDays { get; set; }

    
}
