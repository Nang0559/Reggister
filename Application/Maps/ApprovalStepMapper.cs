

using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Maps
{
    /// <summary>
    /// Map kết hợp giữa Snapshot (quy định bước duyệt) và History (kết quả thực tế)
    /// </summary>
    public static class ApprovalStepMapper
    {
        public static ApprovalStepCalculatedDto ToCalculatedDto(
            F03ApprovalStepSnapshot snapshot,
            F03ApprovalHistory? history,
            bool isCurrentStep = false,
            UserIdentityDto? currentUser = null)
        {
            bool isMyTurn = currentUser != null && snapshot.ApproverCode == currentUser.EmployeeCode;
            bool isAdminOverride = currentUser != null && currentUser.Permission.IsAdmin(); // ⭐ gọi trên int

            return new ApprovalStepCalculatedDto
            {
                Level = snapshot.Level,
                RoleName = snapshot.RoleName,
                IsRequired = snapshot.IsRequired,
                Status = history?.Decision ?? DecisionType.Pending, // ✅ không cast, Decision đã là DecisionType
                ApproverCode = snapshot.ApproverCode,
                ApproverName = history?.ApproverName ?? snapshot.ApproverName,
                Comment = history?.Comment,
                ActionAt = history?.ActionAt,
                IsOverridden = history?.IsOverriddenByAdmin ?? false,
                OverriddenByName = history?.OverriddenByName,
                OverriddenAt = history?.OverriddenAt,
                IsCurrentStep = isCurrentStep,
                IsMyTurn = isMyTurn,
                IsAdminOverride = isAdminOverride
            };
        }

        public static List<ApprovalStepCalculatedDto> MapToCalculatedList(
            IEnumerable<F03ApprovalStepSnapshot> snapshots,
            IEnumerable<F03ApprovalHistory> histories,
            UserIdentityDto? currentUser = null)
        {
            var historyByStepId = histories.ToDictionary(h => h.StepId);
            var orderedSnapshots = snapshots.OrderBy(s => s.Level).ToList();

            var currentLevel = orderedSnapshots
                .Where(s => s.IsRequired &&
                    (!historyByStepId.TryGetValue(s.Id, out var h) || h.Decision == DecisionType.Pending)) // ✅ không cast
                .Select(s => (int?)s.Level)
                .FirstOrDefault();

            return orderedSnapshots.Select(s =>
            {
                historyByStepId.TryGetValue(s.Id, out var history);
                return ToCalculatedDto(s, history, s.Level == currentLevel, currentUser);
            }).ToList();
        }

        public static ApprovalStatus ComputeOverallStatus(IEnumerable<ApprovalStepCalculatedDto> steps)
        {
            var requiredSteps = steps.Where(s => s.IsRequired).ToList();

            if (requiredSteps.Count == 0)
                return ApprovalStatus.Pending;

            if (requiredSteps.Any(s => s.Status == DecisionType.Rejected))
                return ApprovalStatus.Rejected;

            if (requiredSteps.Any(s => s.Status == DecisionType.Returned))
                return ApprovalStatus.NeedsRevision;

            if (requiredSteps.All(s => s.Status == DecisionType.Approved))
                return ApprovalStatus.Approved;

            if (requiredSteps.Any(s => s.Status == DecisionType.Approved))
                return ApprovalStatus.InProgress;

            return ApprovalStatus.Pending;
        }
    }
}
