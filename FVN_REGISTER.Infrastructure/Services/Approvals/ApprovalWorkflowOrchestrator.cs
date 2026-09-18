using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    /// <summary>
    /// Application workflow boundary backed by the Infrastructure approval engine/provider.
    /// Keeps API/Application orchestration independent from EF and provider implementations.
    /// </summary>
    public sealed class ApprovalWorkflowOrchestrator<TSubject>
        : IApprovalWorkflowOrchestrator<TSubject>
        where TSubject : IApprovalSubject
    {
        private readonly IApprovalEngine<TSubject> _engine;
        private readonly IApprovalProvider<TSubject> _provider;
        private readonly IApprovalNotificationService _notification;

        public ApprovalWorkflowOrchestrator(
            IApprovalEngine<TSubject> engine,
            IApprovalProvider<TSubject> provider,
            IApprovalNotificationService notification)
        {
            _engine = engine;
            _provider = provider;
            _notification = notification;
        }

        public async Task InitApprovalAsync(
            int requestId,
            ApprovalBuildContext ctx,
            CancellationToken ct)
        {
            var subject = await _provider.GetSubjectAsync(requestId, ct)
                ?? throw new InvalidOperationException(
                    $"Không tìm thấy request {requestId} để khởi tạo approval snapshot.");

            var snapshot = await _provider.BuildSnapshotAsync(subject, ctx, ct);
            await _engine.InitializeStepsAsync(requestId, snapshot, ct);

            // Submit activates the first required step. Notify it immediately so the
            // first approver receives both email and in-app/SignalR notification.
            var first = await _engine.GetNextStepAsync(requestId, ct);
            if (first == null) return;

            if (!string.IsNullOrWhiteSpace(first.ApproverCode) && !string.IsNullOrWhiteSpace(first.ApproverEmail))
            {
                await _notification.NotifyNewRequestAsync(
                    first.ApproverCode!,
                    first.ApproverEmail!,
                    first.ApproverName ?? string.Empty,
                    requestId,
                    subject.Module,
                    subject.EmployeeName ?? string.Empty,
                    first.Level,
                    ct);
            }

            if (string.IsNullOrWhiteSpace(first.ApproverCode))
                return;

            await _notification.NotifyApproverInAppAsync(
                first.ApproverCode,
                requestId,
                subject.Module,
                subject.EmployeeName ?? string.Empty,
                first.Level,
                ct);
        }

        public Task<ApprovalActionResult> ApproveAsync(
            ApprovalActionDto action,
            CancellationToken ct)
        {
            action.IsReject = false;
            return _engine.ProcessDecisionAsync(action, ct);
        }

        public Task<ApprovalActionResult> RejectAsync(
            ApprovalActionDto action,
            CancellationToken ct)
        {
            action.IsReject = true;
            return _engine.ProcessDecisionAsync(action, ct);
        }

        public Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(
            string approverEmail,
            CancellationToken ct)
            => _engine.GetPendingForApproverAsync(approverEmail, ct);

        public Task<List<ApprovalStepDto>> GetStepsAsync(
            int requestId,
            CancellationToken ct)
            => _engine.GetStepsAsync(requestId, ct);
    }
}
