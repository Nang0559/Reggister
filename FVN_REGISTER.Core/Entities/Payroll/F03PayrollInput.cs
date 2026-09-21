using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FVN_REGISTER.Core.Entities.Payroll;
[Table("F03PayrollInputs")]
public sealed class F03PayrollInput : BaseAuditEntity
{
 public new long Id { get; set; }
 public int PayrollPeriodId { get; set; }
 public int EmployeeId { get; set; }
 public DateOnly WorkDate { get; set; }
 public decimal WorkMinutes { get; set; }
 public decimal LeaveTotal { get; set; }
 public decimal OTMinutes { get; set; }
 [Required,StringLength(30)] public string Source { get; set; }="HRM_CALCULATION";
 public DateTime SnapshotAt { get; set; }
}