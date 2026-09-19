namespace FVN_REGISTER.Contract.Dtos.HrmSync;
public sealed class HrmAttendanceCalculationResultDto
{
 public Guid CalculationBatchId { get; set; }
 public string? DeptCode { get; set; }
 public DateTime FromDate { get; set; }
 public DateTime ToDate { get; set; }
 public int EmployeeCount { get; set; }
 public int CalculatedRows { get; set; }
 public DateTime? StartedAt { get; set; }
 public DateTime? FinishedAt { get; set; }
 public string CalculationVersion { get; set; } = string.Empty;
}
