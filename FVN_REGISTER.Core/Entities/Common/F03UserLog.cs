
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03UserLogs")]
public partial class F03UserLog : BaseAuditEntity
{
    public int UserId { get; set; }

    [Required, StringLength(255)]
    public string LastSeen { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string LastSeenUrl { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ApplicationName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string ApplicationVersion { get; set; } = string.Empty; // Sửa lỗi chính tả

    [Required, StringLength(100)]
    public string WorkstationName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string WorkstationUser { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
