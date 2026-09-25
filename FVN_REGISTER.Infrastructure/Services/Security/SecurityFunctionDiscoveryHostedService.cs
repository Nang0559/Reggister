using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Security;

public sealed class SecurityFunctionDiscoveryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SecurityFunctionDiscoveryHostedService> _logger;

    public SecurityFunctionDiscoveryHostedService(IServiceScopeFactory scopeFactory, ILogger<SecurityFunctionDiscoveryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give the API time to finish startup and database connectivity checks.
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var registry = scope.ServiceProvider.GetRequiredService<SecurityFunctionRegistryService>();
                var result = await registry.ReconcileAsync(stoppingToken);
                _logger.LogInformation(
                    "Security function discovery completed: Discovered={Discovered}, Matched={Matched}, New={New}, Retirement={Retirement}, Conflict={Conflict}",
                    result.Discovered, result.Matched, result.PendingRegistration, result.PendingRetirement, result.Conflict);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Security function discovery failed. Existing permissions were not changed by the discovery process.");
            }

            try { await Task.Delay(TimeSpan.FromHours(6), stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }
}
