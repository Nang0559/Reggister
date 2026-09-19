using FVN_REGISTER.Core.Enums;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.Approvers
{
    [Table("F03ApprovalReminderLog")]
    public class F03ApprovalReminderLog : BaseAuditEntity
    {
        public RequestModule RequestType { get; set; }
        public int RequestId { get; set; }
        public int Level { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
