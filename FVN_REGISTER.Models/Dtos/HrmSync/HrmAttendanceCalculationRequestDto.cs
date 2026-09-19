namespace FVN_REGISTER.Contract.Dtos.HrmSync;
public sealed class HrmAttendanceCalculationRequestDto
{
 public string? DeptCode { get; set; }
 public DateTime FromDate { get; set; } = DateTime.Today;
 public DateTime ToDate { get; set; } = DateTime.Today;
}
