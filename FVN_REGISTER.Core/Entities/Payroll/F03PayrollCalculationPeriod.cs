using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FVN_REGISTER.Core.Entities.Payroll;
[Table("F03PayrollCalculationPeriods")]
public sealed class F03PayrollCalculationPeriod : BaseAuditEntity
{
 public new int Id { get; set; }
 [Required,StringLength(20)] public string PeriodCode { get; set; }=string.Empty;
 public DateOnly FromDate { get; set; }
 public DateOnly ToDate { get; set; }
 [Required,StringLength(20)] public string Status { get; set; }="Open";
 public DateTime? CalculatedAt { get; set; }
 public int? CalculatedBy { get; set; }
 public DateTime? LockedAt { get; set; }
 public int? LockedBy { get; set; }
 public DateTime? ExportedAt { get; set; }
 public int? ExportedBy { get; set; }
}