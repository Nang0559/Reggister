using FVN_REGISTER.Contract.Interfaces.Leaves;

namespace FVN_REGISTER.API.Services.Escalations
{
    // Cần tạo file: EscalationBackgroundWorker.cs
    public class EscalationBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscalationBackgroundWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(4); // Chạy mỗi 4 tiếng

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var escalationService = scope.ServiceProvider
                    .GetRequiredService<ILeaveEscalationService>();

                await escalationService.ProcessAutoEscalationAsync(stoppingToken);

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
