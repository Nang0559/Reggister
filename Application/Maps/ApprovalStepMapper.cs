using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Maps;

public static class ApprovalStepMapper
{
    public static ApprovalStepDto ToDto(
        F03ApprovalStepSnapshot snapshot,
        F03ApprovalHistory? history)
    {
        var decision = history?.Decision ?? DecisionType.Pending;

        return new ApprovalStepDto
        {
            Level = snapshot.Level,
            RoleName = snapshot.RoleName,
            IsRequired = snapshot.IsRequired,
            Decision = decision,
            ApproverCode = snapshot.ApproverCode,
            ApproverName = history?.ApproverName ?? snapshot.ApproverName,
            ApproverEmail = snapshot.ApproverEmail,
            Comment = history?.Comment,
            ApproveTime = history?.ActionAt,
            IsOverriddenByAdmin = history?.IsOverriddenByAdmin ?? false,
            OverriddenByName = history?.OverriddenByName,
            OverriddenAt = history?.ActionAt
        };
    }

    public static List<ApprovalStepDto> MapToList(
        IEnumerable<F03ApprovalStepSnapshot> snapshots,
        IEnumerable<F03ApprovalHistory> histories)
    {
        var historyByStepId = histories.ToDictionary(h => h.StepId);

        return snapshots
            .OrderBy(s => s.Level)
            .Select(s =>
            {
                historyByStepId.TryGetValue(s.Id, out var history);
                return ToDto(s, history);
            })
            .ToList();
    }

    public static ApprovalStatus ComputeOverallStatus(IEnumerable<ApprovalStepDto> steps)
    {
        var requiredSteps = steps.Where(s => s.IsRequired).ToList();

        if (requiredSteps.Count == 0)
            return ApprovalStatus.Pending;

        if (requiredSteps.Any(s => s.Decision == DecisionType.Rejected))
            return ApprovalStatus.Rejected;

        if (requiredSteps.Any(s => s.Decision == DecisionType.Returned))
            return ApprovalStatus.NeedsRevision;

        if (requiredSteps.All(s => s.Decision == DecisionType.Approved))
            return ApprovalStatus.Approved;

        if (requiredSteps.Any(s => s.Decision == DecisionType.Approved))
            return ApprovalStatus.InProgress;

        return ApprovalStatus.Pending;
    }
}