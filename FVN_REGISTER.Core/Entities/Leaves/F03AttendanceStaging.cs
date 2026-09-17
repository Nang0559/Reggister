using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FVN_REGISTER.Core.Entities.Leaves;
[Table("F03AttendanceStaging")]
public partial class F03AttendanceStaging
{
    [Key]
    public int Id { get; set; }

    public DateTime WorkDate { get; set; }

    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [StringLength(20)]
    public string? DeptCode { get; set; }
    [StringLength(100)]
    public string? DeptName { get; set; }
    [StringLength(100)]
    public string? FullName { get; set; }

    public string? CheckInText { get; set; }
    public string? CheckOutText { get; set; }
    public DateTime? CheckInDateTime { get; set; }
    public DateTime? CheckOutDateTime { get; set; }

    [StringLength(20)]
    public string? ShiftCode { get; set; }
    [StringLength(100)]
    public string? ShiftName { get; set; }
    [StringLength(10)]
    public string? ShiftAbbr { get; set; }

    // Đổi tên cho chuyên nghiệp
    public int? ShiftCategory { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? OtHours { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal? TotalHours { get; set; }

    public bool IsHoliday { get; set; } = false;
    [StringLength(50)]
    public string? HolidayType { get; set; }
    [StringLength(20)]
    public string? ShiftType { get; set; }

    public DateTime SyncedAt { get; set; } = DateTime.Now;
}