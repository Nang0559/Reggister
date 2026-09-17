
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;



namespace FVN_REGISTER.Infrastructure.Services.Approvals
{

    // ════════════════════════════════════════════════════════════════════
    // Generic base — Leave và OT đều kế thừa, chỉ khác RequestType
    // ════════════════════════════════════════════════════════════════════
    public abstract class ApprovalEscalationService<TSubject>
       : BaseService<ApprovalEscalationService<TSubject>>, IApprovalEscalationService
       where TSubject : class, IApprovalSubject
    {
        protected readonly IUnitOfWork Uow;
        protected readonly IApprovalProvider<TSubject> Provider;
        private readonly IEscalationRuleService _rule;
        private readonly IWorkingDayService _workingDay;
        private readonly IEmailService _email;

        protected abstract RequestModule ModuleKind { get; }

        protected ApprovalEscalationService(
            IUnitOfWork uow,
            IApprovalProvider<TSubject> provider,
            IEscalationRuleService rule,
            IWorkingDayService workingDay,
            IEmailService email,
            ILogger<ApprovalEscalationService<TSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            Uow = uow;
            Provider = provider;
            _rule = rule;
            _workingDay = workingDay;
            _email = email;
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

                    var changed = await ProcessStepAsync(requestId, currentStep, baseTime, deptCode ?? "ALL", ct);
                    if (changed) anyChanged = true;
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
            int requestId, F03ApprovalStepSnapshot step, DateTime baseTime, string deptCode, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(step.ApproverEmail)) return false;

            var now = DateTime.Now;
            var rule = await _rule.GetRuleAsync(ModuleKind, step.Level, deptCode, ct);

            double elapsedHours = await _workingDay.GetWorkingHoursAsync(baseTime, now);
            DateTime deadline = _rule.GetDeadline(baseTime, rule.DeadlineHour);

            if (now >= deadline)
                return await RecordSystemActionAsync(requestId, step, "AutoReject",
                    $"Quá deadline {rule.DeadlineHour}h", ct);

            if (elapsedHours >= (double)rule.EscalateHours)
                return await RecordSystemActionAsync(requestId, step, "Escalated",
                    $"Quá {rule.EscalateHours}h làm việc", ct);

            if (elapsedHours >= (double)rule.WarningHours)
            {
                var alreadyReminded = await Uow.Repository<F03ApprovalReminderLog>().Query()
                    .AnyAsync(x => x.RequestType == ModuleKind
                                && x.RequestId == requestId
                                && x.Level == step.Level
                                && x.SentAt.Date == DateTime.Today, ct);

                if (alreadyReminded) return false;

                await _email.QueueEmail(step.ApproverEmail, $"{ModuleKind}_REMINDER",
                    new { requestId, step.Level }, ct);

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
            int requestId, F03ApprovalStepSnapshot step, string action, string comment, CancellationToken ct)
        {
            await Uow.Repository<F03ApprovalHistory>().AddAsync(new F03ApprovalHistory
            {
                RequestId = requestId,
                RequestType = ModuleKind,
                StepId = step.Id,
                ApproverCode = SystemUser.Code,
                ApproverName = SystemUser.DisplayName,
                Decision = DecisionType.Rejected,
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

            await _email.QueueEmail(step.ApproverEmail, $"{ModuleKind}_{action.ToUpper()}",
                new { requestId, step.Level }, ct);

            await ApplyStatusViaProviderAsync(requestId, ct);

            return true;
        }

        protected abstract Task<List<int>> GetActiveRequestIdsAsync(CancellationToken ct);
        protected abstract Task<Dictionary<int, DateTime>> GetRegisterDateMapAsync(List<int> ids, CancellationToken ct);
        protected abstract Task<Dictionary<int, string?>> GetDeptCodeMapAsync(List<int> ids, CancellationToken ct);

        private static (F03ApprovalStepSnapshot Step, ApprovalStepCalculatedDto Calculated)? GetCurrentPendingStep(
            List<F03ApprovalStepSnapshot> steps, List<F03ApprovalHistory> histories)
        {
            var calculatedList = ApprovalStepMapper.MapToCalculatedList(steps, histories);

            var currentCalculated = calculatedList.FirstOrDefault(x => x.IsCurrentStep);
            if (currentCalculated == null) return null;
            if (currentCalculated.Status != DecisionType.Pending) return null;

            var step = steps.First(s => s.Level == currentCalculated.Level);
            return (step, currentCalculated);
        }

        private static DateTime GetBaseTime(
            F03ApprovalStepSnapshot currentStep,
            List<F03ApprovalStepSnapshot> allSteps,
            List<F03ApprovalHistory> histories,
            DateTime registerDate)
        {
            if (currentStep.Level <= 1) return registerDate;

            var prevStepId = allSteps
                .Where(s => s.Level == currentStep.Level - 1)
                .Select(s => s.Id)
                .FirstOrDefault();

            if (prevStepId == 0) return registerDate;

            var prevDecision = histories
                .Where(h => h.StepId == prevStepId)
                .OrderByDescending(h => h.ActionAt)
                .FirstOrDefault();

            return prevDecision?.ActionAt ?? registerDate;
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
