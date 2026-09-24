using FVN_REGISTER.Application.Interfaces.Equipment;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

public sealed class EquipmentInspectionBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EquipmentInspectionBackgroundWorker> _logger;

    public EquipmentInspectionBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<EquipmentInspectionBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IEquipmentInspectionService>();
                await service.GenerateScheduledTasksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Equipment inspection worker failed."); }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}