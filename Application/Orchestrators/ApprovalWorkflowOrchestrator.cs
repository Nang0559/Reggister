using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using Microsoft.Extensions.Logging;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Logging;


namespace FVN_REGISTER.Application.Orchestrators
{
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

        // ================= INIT (gọi ngay sau khi entity gốc đã có Id) =================
        public async Task<ServiceResult> InitApprovalAsync(
            int requestId,
            ApprovalBuildContext ctx,
            CancellationToken ct)
        {
            try
            {
                Logger.LogDebugIf(
                    Debug,
                    "[APPROVAL-WF] InitApproval start: {Type} RequestId={Id}",
                    _provider.RequestType,
                    requestId);

                var subject = await _provider.GetSubjectAsync(requestId, ct);
                if (subject == null)
                {
                    var message =
                        $"Không tìm thấy dữ liệu yêu cầu {requestId} ({_provider.RequestType}).";

                    Logger.LogWarnIf(
                        Debug,
                        "[APPROVAL-WF] Subject not found: RequestId={Id}",
                        requestId);

                    return ServiceResult.Fail(message);
                }

                var snapshotResult =
                    await _provider.BuildSnapshotResultAsync(subject, ctx, ct);

                if (!snapshotResult.Success || snapshotResult.Data == null)
                {
                    Logger.LogWarnIf(
                        Debug,
                        "[APPROVAL-WF] Snapshot build failed: RequestId={Id}, Message={Message}",
                        requestId,
                        snapshotResult.Message);

                    return ServiceResult.Fail(
                        snapshotResult.Message ??
                        $"Không thể khởi tạo luồng duyệt cho {_provider.RequestType}.");
                }

                await _engine.InitializeStepsAsync(
                    requestId,
                    snapshotResult.Data,
                    ct);

                Logger.LogInfoIf(
                    Debug,
                    "[APPROVAL-WF] Steps initialized: RequestId={Id}",
                    requestId);

                var nextStep = await _engine.GetNextStepAsync(requestId, ct);
                if (nextStep != null)
                {
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

                return ServiceResult.Ok("Đã khởi tạo luồng phê duyệt.");
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "[APPROVAL-WF] InitApproval error: RequestId={Id}",
                    requestId);

                return ServiceResult.Fail(
                    $"Không thể khởi tạo luồng phê duyệt {_provider.RequestType}.");
            }
        }

        // ================= APPROVE =================
        public Task<ApprovalActionResult> ApproveAsync(ApprovalActionDto action, CancellationToken ct)
        {
            action.IsReject = false;
            return ProcessAsync(action, ct);
        }

        // ================= REJECT =================
        public Task<ApprovalActionResult> RejectAsync(ApprovalActionDto action, CancellationToken ct)
        {
            action.IsReject = true;
            return ProcessAsync(action, ct);
        }

        // ================= PENDING LIST =================
        public Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(string approverEmail, CancellationToken ct)
            => _engine.GetPendingForApproverAsync(approverEmail, ct);

        // ================= STEPS =================
        public Task<List<ApprovalStepDto>> GetStepsAsync(int requestId, CancellationToken ct)
            => _engine.GetStepsAsync(requestId, ct);

        // ================= CORE =================
        private async Task<ApprovalActionResult> ProcessAsync(ApprovalActionDto action, CancellationToken ct)
        {
            if (action.RequestIds == null || action.RequestIds.Count == 0)
                return ApprovalActionResult.Fail("Không có đơn nào được chọn.");

            // RequestModule là enum, không có trạng thái "rỗng" như string.
            // Caller (ApprovalInboxService) luôn set Kind tường minh trước khi gọi,
            // nên chỉ cần so khớp trực tiếp, không cần nhánh "tự điền nếu rỗng".
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
                Logger.LogDebugIf(Debug,
                    "[APPROVAL-WF] {Action} {Type} Ids=[{Ids}] Level={Level} Actor={Actor}",
                    action.IsReject ? "Reject" : "Approve",
                    action.Kind,
                    string.Join(",", action.RequestIds),
                    action.Level,
                    action.ApproverCode);

                var result = await _engine.ProcessDecisionAsync(action, ct);

                Logger.LogInfoIf(Debug,
                    "[APPROVAL-WF] {Action} done: {Success}/{Total} thành công",
                    action.IsReject ? "Reject" : "Approve",
                    result.SuccessCount, result.TotalCount);

                if (result.Errors.Count > 0)
                {
                    Logger.LogWarnIf(Debug, "[APPROVAL-WF] Errors: {Errors}",
                        string.Join(" | ", result.Errors));
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[APPROVAL-WF] ProcessDecision error: Ids=[{Ids}]",
                    string.Join(",", action.RequestIds));

                return ApprovalActionResult.Fail("Lỗi hệ thống khi xử lý phê duyệt.");
            }
        }

        // Notification không bao giờ được làm fail quy trình duyệt chính
        private async Task SafeNotifyAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Logger.LogWarnIf(Debug, "[APPROVAL-WF] Notification failed (ignored): {Msg}", ex.Message);
            }
        }
    }
}
