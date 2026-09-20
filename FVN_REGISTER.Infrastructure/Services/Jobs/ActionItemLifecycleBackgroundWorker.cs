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

                var expired = await db.ActionItems
                    .Where(x => x.IsActive != false
                        && (x.Status == ActionItemStatus.Open || x.Status == ActionItemStatus.InProgress)
                        && x.DueAt.HasValue
                        && x.DueAt.Value <= now)
                    .ToListAsync(stoppingToken);

                foreach (var action in expired)
                {
                    var hasResolvedReconciliation = await db.ExecutionReconciliations
                        .AsNoTracking()
                        .AnyAsync(x => x.IsActive != false
                            && x.ActionId == action.ActionId
                            && x.ReconciliationStatus == "Resolved",
                            stoppingToken);

                    if (hasResolvedReconciliation)
                    {
                        action.Status = ActionItemStatus.Completed;
                        action.CompletedAt = now;
                        action.LastModifiedSource = "ACTION_LIFECYCLE";
                    }
                    else
                    {
                        action.Status = ActionItemStatus.Expired;
                        action.ExpiredAt = now;
                        action.LastModifiedSource = "ACTION_LIFECYCLE";
                    }

                    action.ModifiedAt = now;
                }

                if (expired.Count > 0)
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
