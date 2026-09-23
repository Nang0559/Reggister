using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

public sealed class BackgroundWorkerHealthCheck : IHealthCheck
{
    private readonly BackgroundWorkerHealthRegistry _registry;
    private readonly IConfiguration _configuration;

    public BackgroundWorkerHealthCheck(
        BackgroundWorkerHealthRegistry registry,
        IConfiguration configuration)
    {
        _registry = registry;
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var staleMinutes = Math.Clamp(
            _configuration.GetValue<int?>("BackgroundWorkers:StaleMinutes") ?? 15,
            1,
            1440);

        var now = DateTimeOffset.UtcNow;
        var staleAfter = TimeSpan.FromMinutes(staleMinutes);
        var snapshots = _registry.Snapshot();

        if (snapshots.Count == 0)
            return Task.FromResult(HealthCheckResult.Healthy("No background worker has registered yet."));

        var unhealthy = snapshots
            .Where(x =>
                x.LastSuccessAt == null
                    ? now - x.StartedAt > staleAfter
                    : now - x.LastSuccessAt.Value > staleAfter
                        || (x.LastFailureAt.HasValue
                            && x.LastFailureAt.Value > x.LastSuccessAt.Value
                            && now - x.LastFailureAt.Value > staleAfter))
            .ToList();

        if (unhealthy.Count == 0)
            return Task.FromResult(HealthCheckResult.Healthy(
                "Background workers are reporting within the configured freshness window."));

        var data = unhealthy.ToDictionary(
            x => x.Name,
            x => (object?)new
            {
                x.StartedAt,
                x.LastSuccessAt,
                x.LastFailureAt,
                x.LastError
            });

        return Task.FromResult(HealthCheckResult.Unhealthy(
            "One or more background workers are stale or failing.",
            data: data));
    }
}
