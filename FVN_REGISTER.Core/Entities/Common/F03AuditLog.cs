
using FVN_REGISTER.Core.Entities.Security;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03AuditLogs")]
public partial class F03AuditLog:BaseAuditEntity
{
    public int? UserId { get; set; }

    [StringLength(100)]
    public string? UserName { get; set; } // Lưu lại để tránh join khi cần truy xuất log cũ

    [Required, StringLength(100)]
    public string Action { get; set; } = string.Empty; // Ví dụ: "LOGIN", "DELETE_OT_REQUEST"

    [StringLength(2000)]
    public string? Description { get; set; } // Chi tiết thao tác hoặc đối tượng bị thay đổi

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [StringLength(255)]
    public string? UserAgent { get; set; } // Thông tin trình duyệt/thiết bị

    // Navigation Property
    [ForeignKey("UserId")]
    public virtual F03User? User { get; set; }
}
