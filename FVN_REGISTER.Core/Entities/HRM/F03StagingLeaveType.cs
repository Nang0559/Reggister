using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.HRM
{
    [Table("F03StagingLeaveType")]
    public class F03StagingLeaveType : IHrmStagingEntity
    {
        public int Id { get; set; }
        public string EntityKey { get; set; } = string.Empty;   // = LeaveTypeCode
        public HrmChangeAction Action { get; set; }
        public bool IsProcessed { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string? LeaveTypeName2 { get; set; }
        public bool TinhPhep { get; set; }
        public string? HRMCode
        {
            get; set;
        }
        public bool TinhPhep { get; set; }
    }
}
