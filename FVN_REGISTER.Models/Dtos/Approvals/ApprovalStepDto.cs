

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
        public bool? IsApproved { get; set; }
        public DateTime? ApproveTime { get; set; }
        public string? Comment { get; set; }

        public bool IsRequired { get; set; } = true;
        public bool IsSkipped => !IsRequired && IsApproved == null;

        public bool IsOverriddenByAdmin { get; set; } = false;
        public string? OverriddenByName { get; set; }
        public DateTime? OverriddenAt { get; set; }
        public string? OverrideDescription => IsOverriddenByAdmin ? $"Duyệt thay: {OverriddenByName}" : null;

        public string StatusText => IsApproved == true ? "Đã duyệt" : IsApproved == false ? "Từ chối" : "Chờ duyệt";
        // Không còn StatusColor ở đây
    }
}
