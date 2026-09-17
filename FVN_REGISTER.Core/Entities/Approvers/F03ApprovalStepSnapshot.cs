
using System.ComponentModel.DataAnnotations.Schema;


namespace FVN_REGISTER.Core.Entities.Approvers
{
    [Table("F03ApprovalStepSnapshots")]
    public sealed class F03ApprovalStepSnapshot
    {
        public int Id { get; set; }
        public int SnapshotId { get; init; } // Khóa ngoại liên kết ngược lại với Snapshot
        public int Level { get; init; }

        public string ApproverCode { get; init; } = string.Empty;
        public string ApproverName { get; init; } = string.Empty;
        public string ApproverEmail { get;set; } = string.Empty;
        public string RoleName { get; init; } = string.Empty;

        // Đảm bảo tên trường này khớp với DTO (IsRequired)
        public bool IsRequired { get; init; }
    }
}
