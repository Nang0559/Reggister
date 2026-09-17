using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.OT
{
    [Table("F03OTLimitRules")]
    public partial class F03OTLimitRule : BaseAuditEntity
    {
        // Sử dụng Enum để thay thế cho string "Daily", "Weekly"...
        public OTLimitType LimitType { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal LimitValue { get; set; }

        [StringLength(20)]
        public string? PositionCode { get; set; }

        [StringLength(20)]
        public string? DeptCode { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal LimitHours { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
