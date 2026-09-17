
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Core.Entities.Leaves
{
    public partial class F03LeaveType : BaseAuditEntity
    {
        // LeaveTypeId gốc trong DB chính là Id kế thừa từ BaseAuditEntity (identity PK).
        // Không khai báo lại Id ở đây — giữ nguyên nguyên tắc PK duy nhất từ base như F03OTReasonCode.

        [Required, StringLength(50)]
        public string LeaveTypeCode { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string LeaveTypeName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? LeaveTypeName2 { get; set; }

        public bool IsCountedAsLeave { get; set; }

        [StringLength(50)]
        public string? HRMCode { get; set; }
    }
}
