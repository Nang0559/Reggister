using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Leaves;

[Table("F03LeaveDayDetails")]
public partial class F03LeaveDayDetail : BaseAuditEntity
{
    [ForeignKey(nameof(LeaveDay))]
    public int LeaveDaysId { get; set; }

    public DateTime LeaveDate { get; set; }

    [Required, StringLength(10)]
    public string LeaveTypeCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string? LeaveTypeName { get; set; }

    // Đổi tên cho tường minh hơn
    public bool IsCountedAsLeave { get; set; }

    public bool IsHalfDay { get; set; }

    // Sử dụng Enum thay vì string để tránh gõ sai
    public HalfDayType? HalfDayOption { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DayValue { get; set; }

    // Navigation Property (Navigation tới F03LeaveDay đã chuẩn hóa tên)
    public virtual F03LeaveDay LeaveDay { get; set; } = null!;
}
