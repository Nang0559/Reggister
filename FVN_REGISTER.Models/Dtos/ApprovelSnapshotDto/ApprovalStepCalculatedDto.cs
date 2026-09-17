

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto
{
    public class ApprovalStepCalculatedDto
    {
        public required int Level { get; init; }
        public required string RoleName { get; init; }
        public bool IsRequired { get; init; }
        public DecisionType Status { get; init; }

        public string? ApproverCode { get; init; }
        public string? ApproverName { get; init; }
        public string? Comment { get; init; }
        public DateTime? ActionAt { get; init; }

        public bool IsOverridden { get; init; }
        public string? OverriddenByName { get; init; }
        public DateTime? OverriddenAt { get; init; }

        public bool IsCurrentStep { get; init; }
        public bool IsMyTurn { get; init; }
        public bool IsAdminOverride { get; init; }   // ⭐ mới

        public bool CanIApprove =>
            IsCurrentStep && Status == DecisionType.Pending && (IsMyTurn || IsAdminOverride);
    }
}
