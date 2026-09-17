using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EmailLogs")]
public partial class F03EmailLog
{
    [Key]
    public int Id { get; set; }

    // Liên kết với bảng Queue (nếu cần truy xuất ngược lại)
    public int? QueueId { get; set; }

    [Required, StringLength(255)]
    public string ToEmail { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Subject { get; set; } = string.Empty;

    // Sử dụng lại EmailStatus enum cho thống nhất
    public EmailStatus Status { get; set; }

    public DateTime SentAt { get; set; } = DateTime.Now;

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
}
