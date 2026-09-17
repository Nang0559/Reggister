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

        public ApprovalWorkflowOrchestrator(
            IApprovalEngine<TSubject> engine,
            IApprovalProvider<TSubject> provider)
        {
            _engine = engine;
            _provider = provider;
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

        public Task<List<ApprovalStepCalculatedDto>> GetStepsAsync(
            int requestId,
            CancellationToken ct)
            => _engine.GetStepsAsync(requestId, ct);
    }
}
