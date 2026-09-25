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
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        var interval = TimeSpan.FromHours(6);
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
                interval = TimeSpan.FromHours(6);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                interval = TimeSpan.FromMinutes(5);
                _logger.LogError(ex, "Security function discovery failed. Existing permissions were not changed by the discovery process. Retrying in 5 minutes.");
            }

            try { await Task.Delay(interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }
}
