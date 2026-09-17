using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.OT
{
    [Table("F03OTCodes")]
    public partial class F03OTReasonCode : BaseAuditEntity
    {
        // Id: Nếu bạn dùng ReasonCode làm PK thì có thể xóa Id ở BaseAuditEntity hoặc 
        // chuyển sang dùng ReasonCode làm PK duy nhất. 
        // Ở đây tôi giữ Id làm PK theo chuẩn, còn ReasonCode là mã nghiệp vụ.

        [Required, StringLength(10)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

       
    }
}
