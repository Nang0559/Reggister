using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

public sealed class ActionItemLifecycleBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ActionItemLifecycleBackgroundWorker> _logger;

    public ActionItemLifecycleBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ActionItemLifecycleBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<FVNWEBAPPContext>();
                var now = DateTime.Now;

                var dueActions = await db.ActionItems
                    .Where(x => x.IsActive != false
                        && (x.Status == ActionItemStatus.Open || x.Status == ActionItemStatus.InProgress)
                        && x.DueAt.HasValue
                        && x.DueAt.Value <= now)
                    .ToListAsync(stoppingToken);

                foreach (var action in dueActions)
                {
                    var reconciliation = await db.ExecutionReconciliations
                        .AsNoTracking()
                        .Where(x => x.IsActive != false && x.ActionId == action.ActionId)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (reconciliation?.ReconciliationStatus == "Resolved")
                    {
                        action.Status = ActionItemStatus.Completed;
                        action.CompletedAt = now;
                        action.LastModifiedSource = "ACTION_LIFECYCLE";
                    }
                    else
                    {
                        // An unresolved execution is authoritative: the Action
                        // remains actionable even after DueAt. Never silently expire
                        // the Action while payroll/HR still depends on it.
                        action.Status = ActionItemStatus.InProgress;
                        action.ExpiredAt = null;
                        action.LastModifiedSource = "ACTION_LIFECYCLE";
                    }

                    action.ModifiedAt = now;
                }

                // Repair actions that were expired by an older worker version.
                var orphanedExpired = await db.ActionItems
                    .Where(x => x.IsActive != false
                        && x.Status == ActionItemStatus.Expired)
                    .ToListAsync(stoppingToken);

                foreach (var action in orphanedExpired)
                {
                    var unresolved = await db.ExecutionReconciliations
                        .AsNoTracking()
                        .AnyAsync(x => x.IsActive != false
                            && x.ActionId == action.ActionId
                            && x.ReconciliationStatus != "Resolved",
                            stoppingToken);

                    if (unresolved)
                    {
                        action.Status = ActionItemStatus.InProgress;
                        action.ExpiredAt = null;
                        action.ModifiedAt = now;
                        action.LastModifiedSource = "ACTION_LIFECYCLE_REPAIR";
                    }
                }

                if (dueActions.Count > 0 || orphanedExpired.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Action item lifecycle worker failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
