using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EscalationLogs")]
public partial class F03EscalationLog : BaseAuditEntity
{
    // Id kế thừa từ BaseAuditEntity

    public int RequestId { get; set; }

    // Sử dụng lại Enum RequestModule đã định nghĩa ở bảng Attachment
    public RequestModule RequestModule { get; set; }

    public int Level { get; set; }

    [StringLength(50)]
    public string? Action { get; set; } // Ví dụ: "Escalated", "Notified", "Approved"

    // CreatedAt đã có trong BaseAuditEntity
}
