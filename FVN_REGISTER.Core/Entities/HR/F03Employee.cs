
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.HR;

[Table("F03Employees")]
public partial class F03Employee : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string DeptCode { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PositionCode { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }
    public int? GenderCode { get; set; }

    [Required, StringLength(100)]
    public string EmailAddress { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime? FirstWorkingDate { get; set; }
    public DateTime? EndWorkingDate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalLeaveDays { get; set; }

    public int? EmployeeNo { get; set; }
    public int? LevelApprove { get; set; }

    // 🔥 Navigation tới Position (FK: PositionCode -> F03Position.PositionCode)
    public virtual F03Position? Position { get; set; }

    // 🔥 Navigation tới Department, nếu bạn cũng muốn ràng buộc luôn (khuyến nghị)
    public virtual F03Department? Department { get; set; }
}
