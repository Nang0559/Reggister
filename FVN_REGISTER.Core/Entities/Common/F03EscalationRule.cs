using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EscalationRules")]
public partial class F03EscalationRule : BaseAuditEntity
{
    // Sử dụng lại Enum RequestModule cho đồng nhất
    public RequestModule RequestModule { get; set; }

    public int Level { get; set; }

    [StringLength(20)]
    public string? DeptCode { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal WarningHours { get; set; } // Giờ bắt đầu cảnh báo

    [Column(TypeName = "decimal(5,2)")]
    public decimal EscalateHours { get; set; } // Giờ bắt đầu leo thang (chuyển cấp)

    public int DeadlineHour { get; set; } // Hạn chót xử lý
}
