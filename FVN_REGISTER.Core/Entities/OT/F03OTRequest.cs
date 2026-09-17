using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.OT;
[Table("F03OTRequests")]
public partial class F03OTRequest : BaseRequestEntity
{
    [Required, StringLength(20)]
    public string OTCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string? CreatedByEmail { get; set; }

    // Sử dụng Enum cho ScopeType
    public ScopeType ScopeType { get; set; } = ScopeType.Selected;

    public DateTime OTDate { get; set; }

    // Dùng DateTime thay vì TimeSpan để tránh lỗi mapping/tính toán
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal PlannedHours { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalOTHours { get; set; }

    [Required, StringLength(20)]
    public string OTTypeCode { get; set; } = string.Empty;

    [StringLength(500)]
    public string? OTReasonSummary { get; set; }

    // Navigation
    public virtual ICollection<F03OTEmployee> Employees { get; set; } = new List<F03OTEmployee>();
}
