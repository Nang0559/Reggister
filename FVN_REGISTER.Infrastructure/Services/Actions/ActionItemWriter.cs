using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Models.Actions;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Actions;

public sealed class ActionItemWriter : IActionItemWriter
{
    private readonly FVNWEBAPPContext _db;

    public ActionItemWriter(FVNWEBAPPContext db) => _db = db;

    public async Task<Guid> EnsureOpenAsync(
        ActionItemDraft draft,
        CancellationToken cancellationToken = default)
    {
        var policy = await _db.ActionPolicies.AsNoTracking()
            .Where(x => x.IsActive && x.IsEnabled
                && x.ModuleCode == draft.ModuleCode
                && x.ActionType == draft.ActionType)
            .Select(x => new
            {
                x.DefaultPriority,
                x.DueHours,
                x.TitleTemplate,
                x.SummaryTemplate
            })
            .FirstOrDefaultAsync(cancellationToken);

        var dueAt = draft.DueAt;
        if (!dueAt.HasValue && policy?.DueHours is > 0)
            dueAt = DateTime.Now.AddHours(policy.DueHours.Value);

        var priority = policy?.DefaultPriority ?? draft.Priority;
        var title = string.IsNullOrWhiteSpace(policy?.TitleTemplate) ? draft.Title : policy.TitleTemplate;
        var summary = string.IsNullOrWhiteSpace(policy?.SummaryTemplate) ? draft.Summary : policy.SummaryTemplate;

        var existing = await _db.ActionItems
            .FirstOrDefaultAsync(x =>
                x.ModuleCode == draft.ModuleCode
                && x.SourceType == draft.SourceType
                && x.SourceId == draft.SourceId
                && x.ParticipantId == draft.ParticipantId
                && x.ActionType == draft.ActionType
                && x.AssignedToEmployeeId == draft.AssignedToEmployeeId
                && (x.Status == ActionItemStatus.Open || x.Status == ActionItemStatus.InProgress),
                cancellationToken);

        if (existing is not null)
        {
            existing.Title = title;
            existing.Summary = summary;
            existing.Severity = draft.Severity;
            existing.Priority = priority;
            existing.DueAt = dueAt;
            existing.DetailRoute = draft.DetailRoute;
            existing.ReferenceNo = draft.ReferenceNo;
            existing.PayloadJson = draft.PayloadJson;
            existing.AssignedToUserId = draft.AssignedToUserId;
            existing.ModifiedBy = draft.ActorUserId;
            existing.ModifiedAt = DateTime.Now;
            await _db.SaveChangesAsync(cancellationToken);
            return existing.ActionId;
        }

        var entity = new F03ActionItem
        {
            ActionId = Guid.NewGuid(),
            ModuleCode = draft.ModuleCode,
            SourceType = draft.SourceType,
            SourceId = draft.SourceId,
            ParticipantId = draft.ParticipantId,
            EmployeeId = draft.EmployeeId,
            AssignedToEmployeeId = draft.AssignedToEmployeeId,
            AssignedToUserId = draft.AssignedToUserId,
            WorkDate = draft.WorkDate,
            ActionType = draft.ActionType,
            Title = title,
            Summary = summary,
            Severity = draft.Severity,
            Priority = priority,
            Status = ActionItemStatus.Open,
            DueAt = dueAt,
            DetailRoute = draft.DetailRoute,
            ReferenceNo = draft.ReferenceNo,
            PayloadJson = draft.PayloadJson,
            CreatedBy = draft.ActorUserId ?? 0,
            CreatedAt = DateTime.Now,
            LastModifiedSource = "ACTION_WRITER"
        };

        _db.ActionItems.Add(entity);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return entity.ActionId;
        }
        catch (DbUpdateException)
        {
            _db.Entry(entity).State = EntityState.Detached;

            var winner = await _db.ActionItems
                .AsNoTracking()
                .Where(x =>
                    x.ModuleCode == draft.ModuleCode
                    && x.SourceType == draft.SourceType
                    && x.SourceId == draft.SourceId
                    && x.ParticipantId == draft.ParticipantId
                    && x.ActionType == draft.ActionType
                    && x.AssignedToEmployeeId == draft.AssignedToEmployeeId
                    && (x.Status == ActionItemStatus.Open || x.Status == ActionItemStatus.InProgress))
                .Select(x => (Guid?)x.ActionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (winner.HasValue)
                return winner.Value;

            throw;
        }
    }
}