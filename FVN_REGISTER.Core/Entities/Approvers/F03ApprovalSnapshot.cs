using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.Common
{
    [Table("F03ApprovalSnapshots")]
    public sealed class F03ApprovalSnapshot
    {
        public int Id { get; init; }
        public int RequestId { get; init; }
        public RequestModule RequestType { get; init; }

        // Dùng ICollection cho EF Core quan hệ 1-n
        public ICollection<F03ApprovalStepSnapshot> Steps { get; init; } = new List<F03ApprovalStepSnapshot>();

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }

    
}
