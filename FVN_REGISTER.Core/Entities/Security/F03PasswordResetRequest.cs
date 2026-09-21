using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03PasswordResetRequests")]
public sealed class F03PasswordResetRequest : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? FullName { get; set; }

    [StringLength(50)]
    public string? DeptCode { get; set; }

    [StringLength(1000)]
    public string? RequestNote { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int? ProcessedBy { get; set; }

    [StringLength(255)]
    public string? ProcessorName { get; set; }

    [StringLength(1000)]
    public string? ResultNote { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [StringLength(255)]
    public string? UserAgent { get; set; }
}
