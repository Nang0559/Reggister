using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.HRM
{
    [Table("F03StagingOTType")]
    public class F03StagingOTType : IHrmStagingEntity
    {
        public int Id { get; set; }
        public string EntityKey { get; set; } = string.Empty;   // = OTTypeCode
        public HrmChangeAction Action { get; set; }
        public bool IsProcessed { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public string OTTypeName { get; set; } = string.Empty;
        public string? OTTypeName2 { get; set; }
        public decimal RateMultiplier { get; set; }
        public string? HRMCode { get; set; }
    }
}
