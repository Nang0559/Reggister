using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.HRM
{
    [Table("F03StagingDepartment")]
    public class F03StagingDepartment : IHrmStagingEntity
    {
        public int Id { get; set; }
        public string EntityKey { get; set; } = string.Empty;   // = DeptCode
        public HrmChangeAction Action { get; set; }
        public bool IsProcessed { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string DeptName { get; set; } = string.Empty;
        public string? ParentDeptCode { get; set; }
                public int? DisplayPriority { get; set; }
                public bool ShowInReport { get; set; } = true;
    }
}
