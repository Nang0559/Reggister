


using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    public interface IApprovalProvider<TSubject> where TSubject : IApprovalSubject
    {
        RequestModule RequestType { get; }
        Task<List<ApprovalStepSnapshotDto>> BuildHierarchyAsync(ApprovalBuildContext ctx, CancellationToken ct);
        Task<ApprovalSnapshotDto> BuildSnapshotAsync(TSubject subject, ApprovalBuildContext ctx, CancellationToken ct);
        Task<TSubject?> GetSubjectAsync(int requestId, CancellationToken ct);
        Task<List<TSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct);
        Task ApplyOverallStatusAsync(int requestId, IReadOnlyList<ApprovalStepCalculatedDto> allSteps, CancellationToken ct);
        Task NotifyStepCompletedAsync(TSubject subject, ApprovalStepCalculatedDto completedStep, bool isFullyApproved, CancellationToken ct);

        // ← THÊM: Engine không biết field riêng của Leave (StartDate/TotalDay) hay OT (OTDate/TotalOTHours),
        // nên mỗi Provider tự map subject của mình sang DTO hiển thị chung cho danh sách chờ duyệt.
        Task<PendingApprovalItemDto> ToPendingItemAsync(
            TSubject subject, List<ApprovalStepCalculatedDto> steps, bool canApprove, CancellationToken ct);
    }
}
