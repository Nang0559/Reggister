using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public class ApprovalEngine<TSubject> : IApprovalEngine<TSubject>
    where TSubject : IApprovalSubject
{
    private readonly IUnitOfWork _uow;
    private readonly IApprovalProvider<TSubject> _provider;
    private readonly IApprovalNotificationService _notification;

    public ApprovalEngine(
        IUnitOfWork uow,
        IApprovalProvider<TSubject> provider,
        IApprovalNotificationService notification)
    {
        _uow = uow;
        _provider = provider;
        _notification = notification;
    }

    private RequestModule Module => _provider.RequestType;

    public async Task InitializeStepsAsync(
        int requestId, ApprovalSnapshotDto snapshot, CancellationToken ct)
    {
        var entity = new F03ApprovalSnapshot
        {
            RequestId = requestId,
            RequestType = Module,
            CreatedAt = snapshot.CapturedAt,
            Steps = snapshot.Steps
                .OrderBy(s => s.Level)
                .Select(s => new F03ApprovalStepSnapshot
                {
                    Level = s.Level,
                    ApproverCode = s.ApproverCode ?? string.Empty,
                    ApproverName = s.ApproverName ?? string.Empty,
                    ApproverEmail = s.ApproverEmail,
                    RoleName = s.RoleName,
                    IsRequired = s.IsRequired
                })
                .ToList()
        };

        await _uow.Repository<F03ApprovalSnapshot>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<ApprovalActionResult> ProcessDecisionAsync(
        ApprovalActionDto action, CancellationToken ct)
    {
        var builder = new ApprovalActionResult.Builder().SetTotal(action.RequestIds.Count);
        var processedIds = new List<int>();

        foreach (var requestId in action.RequestIds)
        {
            var snapshot = await _uow.Repository<F03ApprovalSnapshot>()
                .Query()
                .Include(s => s.Steps)
                .FirstOrDefaultAsync(
                    s => s.RequestId == requestId && s.RequestType == Module, ct);

            var step = snapshot?.Steps.FirstOrDefault(s => s.Level == action.Level);
            if (step == null)
            {
                builder.AddError(requestId, "Không tìm thấy bước duyệt tương ứng.");
                continue;
            }

            var already = await _uow.Repository<F03ApprovalHistory>()
                .Query()
                .AnyAsync(h =>
                    h.RequestId == requestId &&
                    h.RequestType == Module &&
                    h.StepId == step.Id, ct);
            if (already)
            {
                builder.AddError(requestId, "Đơn đã được xử lý trước đó.");
                continue;
            }

            var isOverride = !string.Equals(
                action.ApproverCode, step.ApproverCode, StringComparison.OrdinalIgnoreCase);

            if (isOverride && !action.ApproverPermission.IsAdmin())
            {
                builder.AddError(requestId, "Bạn không có quyền duyệt thay người khác.");
                continue;
            }

            var history = new F03ApprovalHistory
            {
                RequestType = Module,
                RequestId = requestId,
                StepId = step.Id,
                ApproverCode = action.ApproverCode,
                ApproverName = step.ApproverName,
                Decision = action.IsReject ? DecisionType.Rejected : DecisionType.Approved,
                Comment = action.Comment,
                ActionAt = DateTime.Now,
                IsOverriddenByAdmin = isOverride
            };

            await _uow.Repository<F03ApprovalHistory>().AddAsync(history, ct);
            builder.AddSuccess(requestId);
            processedIds.Add(requestId);
        }

        await _uow.SaveChangesAsync(ct);

        foreach (var requestId in processedIds)
        {
            var stepsFull = await GetStepsAsync(requestId, ct);
            await _provider.ApplyOverallStatusAsync(requestId, stepsFull, ct);

            var subject = await _provider.GetSubjectAsync(requestId, ct);
            var completedStep = stepsFull.FirstOrDefault(s => s.Level == action.Level);

            if (subject != null && completedStep != null)
            {
                var isFullyApproved = subject.OverallStatus == ApprovalStatus.Approved;
                await _provider.NotifyStepCompletedAsync(
                    subject, completedStep, isFullyApproved, ct);

                // Normal approval activates the next snapshot step. The activation has
                // exactly the same email + in-app semantics as escalation. This prevents
                // a request from becoming Pending at level N+1 without notifying its owner.
                if (!action.IsReject && !isFullyApproved)
                    await NotifyNextApproverAsync(requestId, subject, ct);
            }
        }

        return builder.Build();
    }

    private async Task NotifyNextApproverAsync(
        int requestId,
        TSubject subject,
        CancellationToken ct)
    {
        var next = await GetNextStepAsync(requestId, ct);
        if (next == null) return;

        if (!string.IsNullOrWhiteSpace(next.ApproverEmail))
        {
            await _notification.NotifyNewRequestAsync(
                next.ApproverCode,
                next.ApproverEmail,
                next.ApproverName,
                requestId,
                Module,
                subject.EmployeeName ?? string.Empty,
                next.Level,
                ct);
        }

        await _notification.NotifyApproverInAppAsync(
            next.ApproverCode,
            requestId,
            Module,
            subject.EmployeeName ?? string.Empty,
            next.Level,
            ct);
    }

    public async Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(
        string approverEmail, CancellationToken ct)
    {
        var approverCode = await _uow.Repository<F03Employee>()
            .Query()
            .Where(e => e.EmailAddress == approverEmail)
            .Select(e => e.EmployeeCode)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(approverCode)) return new();

        var snapshots = await _uow.Repository<F03ApprovalSnapshot>()
            .Query()
            .Include(s => s.Steps)
            .Where(s => s.RequestType == Module &&
                        s.Steps.Any(step => step.ApproverCode == approverCode))
            .ToListAsync(ct);

        var result = new List<PendingApprovalItemDto>();

        foreach (var snapshot in snapshots)
        {
            var myStep = snapshot.Steps.FirstOrDefault(
                s => s.ApproverCode == approverCode);
            if (myStep == null) continue;

            var calculated = await GetStepsAsync(snapshot.RequestId, ct);
            var myCalc = calculated.FirstOrDefault(c => c.Level == myStep.Level);
            if (myCalc == null || myCalc.Status != DecisionType.Pending) continue;

            var previousDone = calculated
                .Where(c => c.Level < myStep.Level && c.IsRequired)
                .All(c => c.Status == DecisionType.Approved);
            if (!previousDone) continue;

            var subject = await _provider.GetSubjectAsync(snapshot.RequestId, ct);
            if (subject == null) continue;

            var item = await _provider.ToPendingItemAsync(
                subject, calculated, canApprove: true, ct);
            result.Add(item);
        }

        return result;
    }

    public async Task<List<ApprovalStepCalculatedDto>> GetStepsAsync(
        int requestId, CancellationToken ct)
    {
        var snapshot = await _uow.Repository<F03ApprovalSnapshot>()
            .Query()
            .Include(s => s.Steps)
            .FirstOrDefaultAsync(
                s => s.RequestId == requestId && s.RequestType == Module, ct);

        if (snapshot == null) return new List<ApprovalStepCalculatedDto>();

        var histories = await _uow.Repository<F03ApprovalHistory>()
            .Query()
            .Where(h => h.RequestId == requestId && h.RequestType == Module)
            .ToListAsync(ct);

        return ApprovalStepMapper.MapToCalculatedList(snapshot.Steps, histories);
    }

    public async Task<ApprovalStepDto?> GetNextStepAsync(
        int requestId, CancellationToken ct)
    {
        var snapshot = await _uow.Repository<F03ApprovalSnapshot>()
            .Query()
            .Include(s => s.Steps)
            .FirstOrDefaultAsync(
                s => s.RequestId == requestId && s.RequestType == Module, ct);

        if (snapshot == null) return null;

        var calculated = await GetStepsAsync(requestId, ct);
        var next = calculated
            .Where(s => s.IsRequired && s.Status == DecisionType.Pending)
            .OrderBy(s => s.Level)
            .FirstOrDefault();

        if (next == null) return null;

        var snapshotStep = snapshot.Steps.FirstOrDefault(s => s.Level == next.Level);
        if (snapshotStep == null) return null;

        return new ApprovalStepDto
        {
            Level = snapshotStep.Level,
            RoleName = snapshotStep.RoleName,
            ApproverCode = snapshotStep.ApproverCode,
            ApproverName = snapshotStep.ApproverName,
            ApproverEmail = snapshotStep.ApproverEmail,
            IsRequired = snapshotStep.IsRequired
        };
    }
}
