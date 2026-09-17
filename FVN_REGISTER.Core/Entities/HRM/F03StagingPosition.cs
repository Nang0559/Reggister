using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.HRM
{
    [Table("F03StagingPosition")]
    public class F03StagingPosition : IHrmStagingEntity
    {
        public int Id { get; set; }
        public string EntityKey { get; set; } = string.Empty;   // = PositionCode
        public HrmChangeAction Action { get; set; }
        public bool IsProcessed { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public int? DefaultApproveLevel { get; set; }
    }
}
