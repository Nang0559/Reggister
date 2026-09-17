using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Application.Orchestrators
{
    /// <summary>
    /// Application orchestration boundary for the approval workflow.
    /// Infrastructure provides the provider/engine implementations; this class
    /// coordinates them without depending on EF, DbContext or Infrastructure types.
    /// </summary>
    public class ApprovalWorkflowOrchestrator<TSubject>
        : BaseService<ApprovalWorkflowOrchestrator<TSubject>>, IApprovalWorkflowOrchestrator<TSubject>
        where TSubject : class, IApprovalSubject
    {
        private readonly IApprovalProvider<TSubject> _provider;
        private readonly IApprovalEngine<TSubject> _engine;
        private readonly IApprovalNotificationService _notification;

        public ApprovalWorkflowOrchestrator(
            IApprovalProvider<TSubject> provider,
            IApprovalEngine<TSubject> engine,
            IApprovalNotificationService notification,
            ILogger<ApprovalWorkflowOrchestrator<TSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _provider = provider;
            _engine = engine;
            _notification = notification;
        }

        public async Task InitApprovalAsync(int requestId, ApprovalBuildContext ctx, CancellationToken ct)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[APPROVAL-WF] InitApproval start: {Type} RequestId={Id}",
                    _provider.RequestType, requestId);

                var subject = await _provider.GetSubjectAsync(requestId, ct);
                if (subject == null)
                    throw new InvalidOperationException(
                        $"Không tìm thấy subject cho RequestId={requestId} ({_provider.RequestType}).");

                var snapshot = await _provider.BuildSnapshotAsync(subject, ctx, ct);
                await _engine.InitializeStepsAsync(requestId, snapshot, ct);

                var nextStep = await _engine.GetNextStepAsync(requestId, ct);
                if (nextStep == null)
                    return;

                var creatorName = subject.EmployeeName ?? subject.EmployeeCode;

                if (!string.IsNullOrEmpty(nextStep.ApproverEmail))
                {
                    await SafeNotifyAsync(() => _notification.NotifyNewRequestAsync(
                        approverCode: nextStep.ApproverCode ?? "",
                        approverEmail: nextStep.ApproverEmail!,
                        approverName: nextStep.ApproverName ?? nextStep.LevelLabel,
                        requestId: requestId,
                        requestType: _provider.RequestType,
                        creatorName: creatorName,
                        level: nextStep.Level,
                        ct: ct));
                }

                if (!string.IsNullOrEmpty(nextStep.ApproverCode))
                {
                    await SafeNotifyAsync(() => _notification.NotifyApproverInAppAsync(
                        approverEmployeeCode: nextStep.ApproverCode!,
                        requestId: requestId,
                        requestType: _provider.RequestType,
                        creatorName: creatorName,
                        level: nextStep.Level,
                        ct: ct));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[APPROVAL-WF] InitApproval error: RequestId={Id}", requestId);
                throw;
            }
        }

        public Task<ApprovalActionResult> ApproveAsync(ApprovalActionDto action, CancellationToken ct)
        {
            action.IsReject = false;
            return ProcessAsync(action, ct);
        }

        public Task<ApprovalActionResult> RejectAsync(ApprovalActionDto action, CancellationToken ct)
        {
            action.IsReject = true;
            return ProcessAsync(action, ct);
        }

        public Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(
            string approverEmail, CancellationToken ct)
            => _engine.GetPendingForApproverAsync(approverEmail, ct);

        public Task<List<ApprovalStepCalculatedDto>> GetStepsAsync(
            int requestId, CancellationToken ct)
            => _engine.GetStepsAsync(requestId, ct);

        private async Task<ApprovalActionResult> ProcessAsync(
            ApprovalActionDto action, CancellationToken ct)
        {
            if (action.RequestIds.Count == 0)
                return ApprovalActionResult.Fail("Không có đơn nào được chọn.");

            if (action.Kind != _provider.RequestType)
            {
                Logger.LogWarnIf(Debug,
                    "[APPROVAL-WF] Kind mismatch: expected {Expected}, got {Actual}",
                    _provider.RequestType, action.Kind);

                return ApprovalActionResult.Fail(
                    $"Loại đơn không khớp: yêu cầu {_provider.RequestType}, nhận {action.Kind}.");
            }

            try
            {
                var result = await _engine.ProcessDecisionAsync(action, ct);

                if (result.Errors.Count > 0)
                    Logger.LogWarnIf(Debug, "[APPROVAL-WF] Errors: {Errors}", string.Join(" | ", result.Errors));

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[APPROVAL-WF] ProcessDecision error: Ids=[{Ids}]",
                    string.Join(",", action.RequestIds));
                return ApprovalActionResult.Fail("Lỗi hệ thống khi xử lý phê duyệt.");
            }
        }

        private async Task SafeNotifyAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "[APPROVAL-WF] Notification failed and was ignored.");
            }
        }
    }
}
