using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.Common
{
    [Table("F03BusinessRules")]
    public partial class F03BusinessRule : BaseAuditEntity
    {
       
        [Required]
        public RequestModule Module { get; set; } // Enum: Leave, Overtime, Trip

        [Required, StringLength(50)]
        public string Code { get; set; } = string.Empty; // Mã loại (VD: OT_NORMAL)

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Lưu các cấu hình đặc thù dưới dạng JSON (vd: {"Rate": 1.5} hoặc {"IsCounted": true})
        [Column(TypeName = "nvarchar(MAX)")]
        public string? ConfigJson { get; set; }

        [StringLength(50)]
        public string? HrmCode { get; set; } // Mã đồng bộ hệ thống HRM
    }
}
