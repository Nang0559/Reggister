using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Core.Entities
{
    public abstract class BaseRequestEntity : BaseAuditEntity, IHistoryRequestEntity
    {
        [Required, StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;
        [StringLength(20)]
        public string? DeptCode { get; set; }
        public ApprovalStatus RequestStatus { get; set; } = ApprovalStatus.Draft;
     
    }
}
