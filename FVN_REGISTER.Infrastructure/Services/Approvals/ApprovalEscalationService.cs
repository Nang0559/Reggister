using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    /// <summary>
    /// Generic timeout/escalation pipeline shared by Leave and Overtime.
    /// </summary>
    public abstract class ApprovalEscalationService<TSubject>
        : BaseService<ApprovalEscalationService<TSubject>>, IApprovalEscalationService
        where TSubject : class, IApprovalSubject
    {
        protected readonly IUnitOfWork Uow;
        protected readonly IApprovalProvider<TSubject> Provider;
        private readonly IEscalationRuleService _rule;
        private readonly IWorkingDayService _workingDay;
        private readonly IEmailService _email;
        private readonly IApprovalNotificationService _notification;
        private readonly IEmployeeUserResolver _userResolver;

        protected abstract RequestModule ModuleKind { get; }

        protected ApprovalEscalationService(
            IUnitOfWork uow,
            IApprovalProvider<TSubject> provider,
            IEscalationRuleService rule,
            IWorkingDayService workingDay,
            IEmailService email,
            IApprovalNotificationService notification,
            IEmployeeUserResolver userResolver,
            ILogger<ApprovalEscalationService<TSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            Uow = uow;
            Provider = provider;
            _rule = rule;
            _workingDay = workingDay;
            _email = email;
            _notification = notification;
            _userResolver = userResolver;
        }

        public async Task ProcessAsync(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ESC] Start | Type={Type}", ModuleKind);

                var activeIds = await GetActiveRequestIdsAsync(ct);
                if (activeIds.Count == 0) return;

                var parentSnapshots = await Uow.Repository<F03ApprovalSnapshot>().Query()
                    .Where(x => x.RequestType == ModuleKind && activeIds.Contains(x.RequestId))
                    .Include(x => x.Steps)
                    .ToListAsync(ct);

                if (parentSnapshots.Count == 0) return;

                var stepRows = parentSnapshots
                    .SelectMany(parent => parent.Steps
                        .Where(s => s.IsRequired)
                        .Select(s => new { parent.RequestId, Step = s }))
                    .OrderBy(x => x.RequestId).ThenBy(x => x.Step.Level)
                    .ToList();

                if (stepRows.Count == 0) return;

                var histories = await Uow.Repository<F03ApprovalHistory>().Query()
                    .Where(x => x.RequestType == ModuleKind && activeIds.Contains(x.RequestId))
                    .ToListAsync(ct);

                var historyByRequest = histories.ToLookup(x => x.RequestId);
                var registerDateMap = await GetRegisterDateMapAsync(activeIds, ct);
                var deptCodeMap = await GetDeptCodeMapAsync(activeIds, ct);

                bool anyChanged = false;

                foreach (var requestId in stepRows.Select(x => x.RequestId).Distinct())
                {
                    ct.ThrowIfCancellationRequested();

                    var stepsOfRequest = stepRows
                        .Where(x => x.RequestId == requestId)
                        .Select(x => x.Step)
                        .OrderBy(x => x.Level)
                        .ToList();

                    var historiesOfRequest = historyByRequest[requestId].ToList();
                    var current = GetCurrentPendingStep(stepsOfRequest, historiesOfRequest);
                    if (current == null) continue;

                    var currentStep = current.Value.Step;
                    if (!registerDateMap.TryGetValue(requestId, out var regDate))
                    {
                        Logger.LogWarnIf(Debug, "[ESC] Missing RegisterDate for RequestId={Id}", requestId);
                        continue;
                    }

                    deptCodeMap.TryGetValue(requestId, out var deptCode);
                    var baseTime = GetBaseTime(currentStep, stepsOfRequest, historiesOfRequest, regDate);

                    if (await ProcessStepAsync(
                        requestId, currentStep, baseTime, deptCode ?? ApproveForDept.All, ct))
                        anyChanged = true;
                }

                if (anyChanged)
                    await Uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[ESC] Done | Type={Type} | RequestCount={Count}",
                    ModuleKind, stepRows.Select(x => x.RequestId).Distinct().Count());
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarnIf(Debug, "[ESC] Operation Cancelled | Type={Type}", ModuleKind);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ESC] Error | Type={Type}", ModuleKind);
                throw;
            }
        }

        private async Task<bool> ProcessStepAsync(
            int requestId,
            F03ApprovalStepSnapshot step,
            DateTime baseTime,
            string deptCode,
            CancellationToken ct)
        {
            var now = DateTime.Now;
            var rule = await _rule.GetRuleAsync(ModuleKind, step.Level, deptCode, ct);
            if (rule == null) return false;

            var elapsedHours = await _workingDay.GetWorkingHoursAsync(baseTime, now);
            var deadline = _rule.GetDeadline(baseTime, rule.DeadlineHour);

            // Hard deadline remains a business rejection. It is deliberately different
            // from Escalated, which transfers the approval responsibility upward.
            if (now >= deadline)
            {
                return await RecordSystemActionAsync(
                    requestId, step, "AutoReject",
                    $"Quá deadline {rule.DeadlineHour}h", DecisionType.Rejected, ct);
            }

            if (elapsedHours >= (double)rule.EscalateHours)
            {
                return await RecordSystemActionAsync(
                    requestId, step, "Escalated",
                    $"Quá {rule.EscalateHours}h làm việc", DecisionType.Escalated, ct);
            }

            if (elapsedHours >= (double)rule.WarningHours)
            {
                if (string.IsNullOrWhiteSpace(step.ApproverEmail)) return false;

                var alreadyReminded = await Uow.Repository<F03ApprovalReminderLog>().Query()
                    .AnyAsync(x => x.RequestType == ModuleKind
                                && x.RequestId == requestId
                                && x.Level == step.Level
                                && x.SentAt.Date == now.Date, ct);
                if (alreadyReminded) return false;

                await _email.QueueEmail(
                    step.ApproverEmail,
                    $"{ModuleKind}_REMINDER",
                    new { requestId, step.Level },
                    ct);

                await Uow.Repository<F03ApprovalReminderLog>().AddAsync(new F03ApprovalReminderLog
                {
                    RequestType = ModuleKind,
                    RequestId = requestId,
                    Level = step.Level,
                    SentAt = now
                }, ct);

                return true;
            }

            return false;
        }

        private async Task<bool> RecordSystemActionAsync(
            int requestId,
            F03ApprovalStepSnapshot step,
            string action,
            string comment,
            DecisionType decision,
            CancellationToken ct)
        {
            await Uow.Repository<F03ApprovalHistory>().AddAsync(new F03ApprovalHistory
            {
                RequestId = requestId,
                RequestType = ModuleKind,
                StepId = step.Id,
                ApproverCode = SystemUser.Code,
                ApproverName = SystemUser.DisplayName,
                Decision = decision,
                Comment = $"[{action}] {comment}",
                ActionAt = DateTime.Now,
                CreatedBy = SystemUser.Id
            }, ct);

            await Uow.Repository<F03EscalationLog>().AddAsync(new F03EscalationLog
            {
                RequestId = requestId,
                RequestModule = ModuleKind,
                Level = step.Level,
                Action = action,
                CreatedBy = SystemUser.Id
            }, ct);

            // Persist the transition before resolving the next step. This makes repeated
            // worker ticks idempotent because the old step is no longer Pending.
            await Uow.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(step.ApproverEmail))
            {
                await _email.QueueEmail(
                    step.ApproverEmail,
                    $"{ModuleKind}_{action.ToUpperInvariant()}",
                    new { requestId, step.Level },
                    ct);
            }

            await ApplyStatusViaProviderAsync(requestId, ct);

            if (decision == DecisionType.Escalated)
                await NotifyNextApproverAsync(requestId, step, ct);

            return true;
        }

        private async Task NotifyNextApproverAsync(
            int requestId,
            F03ApprovalStepSnapshot escalatedStep,
            CancellationToken ct)
        {
            var snapshot = await Uow.Repository<F03ApprovalSnapshot>().Query()
                .Where(x => x.RequestType == ModuleKind && x.RequestId == requestId)
                .Include(x => x.Steps)
                .FirstOrDefaultAsync(ct);
            if (snapshot == null) return;

            var histories = await Uow.Repository<F03ApprovalHistory>().Query()
                .Where(x => x.RequestType == ModuleKind && x.RequestId == requestId)
                .ToListAsync(ct);

            var calculated = ApprovalStepMapper.MapToList(
                snapshot.Steps.OrderBy(x => x.Level).ToList(), histories);

            var next = calculated
                 .Where(x => x.IsRequired && x.Decision == DecisionType.Pending)
                .OrderBy(x => x.Level)
                .FirstOrDefault();
            if (next == null) return;

            var nextStep = snapshot.Steps.FirstOrDefault(x => x.Level == next.Level);
            if (nextStep == null) return;

            var subject = await Provider.GetSubjectAsync(requestId, ct);
            var creatorName = subject?.EmployeeName ?? string.Empty;

            // Email uses the existing REQUEST_NEW template contract so escalation does not
            // require a new template row just to activate the next approver.
            if (!string.IsNullOrWhiteSpace(nextStep.ApproverEmail))
            {
                await _notification.NotifyNewRequestAsync(
                    nextStep.ApproverCode,
                    nextStep.ApproverEmail,
                    nextStep.ApproverName,
                    requestId,
                    ModuleKind,
                    creatorName,
                    nextStep.Level,
                    ct);
            }

            // One in-app notification only: escalation-specific action (not a second
            // generic Pending notification for the same activation).
            var oldUserId = await _userResolver.ResolveUserIdAsync(escalatedStep.ApproverCode, ct);
            var newUserId = await _userResolver.ResolveUserIdAsync(nextStep.ApproverCode, ct);
            if (newUserId is > 0)
            {
                await _notification.NotifyEscalatedInAppAsync(
                    oldUserId ?? 0,
                    newUserId.Value,
                    nextStep.ApproverCode,
                    requestId,
                    ModuleKind,
                    ct);
            }
        }

        protected abstract Task<List<int>> GetActiveRequestIdsAsync(CancellationToken ct);
        protected abstract Task<Dictionary<int, DateTime>> GetRegisterDateMapAsync(List<int> ids, CancellationToken ct);
        protected abstract Task<Dictionary<int, string?>> GetDeptCodeMapAsync(List<int> ids, CancellationToken ct);

        private static (F03ApprovalStepSnapshot Step, ApprovalStepDto Calculated)? GetCurrentPendingStep(
            List<F03ApprovalStepSnapshot> steps,
            List<F03ApprovalHistory> histories)
        {
            var calculatedList = ApprovalStepMapper.MapToList(steps, histories);
            var currentCalculated = calculatedList.FirstOrDefault(x => x.IsCurrentStep);
            if (currentCalculated == null || currentCalculated.Status != DecisionType.Pending)
                return null;

            var step = steps.First(s => s.Level == currentCalculated.Level);
            return (step, currentCalculated);
        }

        private static DateTime GetBaseTime(
            F03ApprovalStepSnapshot currentStep,
            List<F03ApprovalStepSnapshot> allSteps,
            List<F03ApprovalHistory> histories,
            DateTime registerDate)
        {
            // Level numbers are business identifiers, not necessarily contiguous.
            // OT currently uses 3 -> 5 -> 6 -> 7, so searching for Level - 1 would
            // incorrectly restart the timeout clock from RegisterDate at levels 5/6/7.
            var previousStep = allSteps
                .Where(s => s.IsRequired && s.Level < currentStep.Level)
                .OrderByDescending(s => s.Level)
                .FirstOrDefault();

            if (previousStep == null) return registerDate;

            var previousDecision = histories
                .Where(h => h.StepId == previousStep.Id)
                .OrderByDescending(h => h.ActionAt)
                .FirstOrDefault();

            return previousDecision?.ActionAt ?? registerDate;
        }

        private async Task ApplyStatusViaProviderAsync(int requestId, CancellationToken ct)
        {
            var parent = await Uow.Repository<F03ApprovalSnapshot>().Query()
                .Where(x => x.RequestType == ModuleKind && x.RequestId == requestId)
                .Include(x => x.Steps)
                .FirstOrDefaultAsync(ct);
            if (parent == null) return;

            var histories = await Uow.Repository<F03ApprovalHistory>().Query()
                .Where(x => x.RequestType == ModuleKind && x.RequestId == requestId)
                .ToListAsync(ct);

            var calculated = ApprovalStepMapper.MapToCalculatedList(
                parent.Steps.OrderBy(x => x.Level).ToList(), histories);

            await Provider.ApplyOverallStatusAsync(requestId, calculated, ct);
        }
    }
}
