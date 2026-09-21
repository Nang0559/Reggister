using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.HRM;

[Table("F03HrmAttendanceCalculated")]
public sealed class F03HrmAttendanceCalculated
{
    [Key]
    public long Id { get; set; }
    public Guid CalculationBatchId { get; set; }
    public string CalculationVersion { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public int HrmEmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public string? DeptCode { get; set; }
    public string? ShiftAbbr { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int WorkMinutesDay { get; set; }
    public int WorkMinutesNight { get; set; }
    public int OTRecognizedMinutesDay { get; set; }
    public int OTRecognizedMinutesNight { get; set; }
    public bool? HrmHoliday { get; set; }
    public string? AttendanceDisplayValue { get; set; }
    public string? OtDisplayValue { get; set; }
}