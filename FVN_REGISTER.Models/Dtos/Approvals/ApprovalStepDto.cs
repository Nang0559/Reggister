

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApprovalStepDto
    {
        public int Level { get; set; }
        public string? RoleName { get; set; }
        public string LevelLabel => RoleName ?? $"Cấp {Level}";

        public string? ApproverCode { get; set; }
        public string? ApproverName { get; set; }
        public string? ApproverEmail { get; set; }
        public DecisionType Decision { get; set; } = DecisionType.Pending;
        public bool? IsApproved
        {
            get => Decision switch
            {
                DecisionType.Approved => true,
                DecisionType.Rejected => false,
                _ => null
            };
            set { }
        }
        public DateTime? ApproveTime { get; set; }
        public string? Comment { get; set; }

        public bool IsRequired { get; set; } = true;
        public bool IsSkipped => !IsRequired && IsApproved == null;

        public bool IsOverriddenByAdmin { get; set; } = false;
        public string? OverriddenByName { get; set; }
        public DateTime? OverriddenAt { get; set; }
        public string? OverrideDescription => IsOverriddenByAdmin ? $"Duyệt thay: {OverriddenByName}" : null;

        public string StatusText => Decision switch
        {
            DecisionType.Approved => "Đã duyệt",
            DecisionType.Rejected => "Từ chối",
            DecisionType.Returned => "Yêu cầu chỉnh sửa",
            DecisionType.Escalated => "Đã chuyển cấp",
            _ => "Chờ duyệt"
        };
        // Không còn StatusColor ở đây
    }
}
