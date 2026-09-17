
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.OT
{
    [Table("F03OTTypes")]
    public partial class F03OTType : BaseAuditEntity
    {
        [Required, StringLength(50)]
        public string OTTypeCode { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string OTTypeName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? OTTypeName2 { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal RateMultiplier { get; set; } = 1.5m;

        [StringLength(50)]
        public string? HRMCode { get; set; }
    }
}
