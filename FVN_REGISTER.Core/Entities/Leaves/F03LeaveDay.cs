
using FVN_REGISTER.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Leaves;
[Table("F03LeaveDays")]
public partial class F03LeaveDay : BaseRequestEntity, IRequestPeriod
{
    // [Id, EmployeeCode, RequestStatus, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt]
    // đều đã được kế thừa từ BaseRequestEntity

    public int WorkYear { get; set; }

    // IRequestPeriod implementation
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalDay { get; set; }

    public int RegisterId { get; set; }

    [StringLength(500)]
    public string LeaveReason { get; set; } = string.Empty;

    public bool? Sync { get; set; }
    public DateTime? LastSync { get; set; }

    [StringLength(10)]
    public string? LeaveTypeCode { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? TotalLeaveDay { get; set; }

    // Navigation
    public virtual ICollection<F03LeaveDayDetail> F03LeaveDayDetails { get; set; } = new List<F03LeaveDayDetail>();

    // Mapping helper cho StartTime/EndTime khớp với RequestPeriod
    [NotMapped]
    public DateTime StartDate { get => StartTime; set => StartTime = value; }
    [NotMapped]
    public DateTime EndDate { get => EndTime; set => EndTime = value; }
}
