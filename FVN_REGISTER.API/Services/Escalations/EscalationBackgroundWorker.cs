using FVN_REGISTER.Contract.Interfaces.Leaves;

namespace FVN_REGISTER.API.Services.Escalations
{
    // Cần tạo file: EscalationBackgroundWorker.cs
    public class EscalationBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscalationBackgroundWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(4);

        // Constructor phải nhận IServiceProvider từ DI — không new thủ công
        public EscalationBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<EscalationBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ESC WORKER] Started, interval={Hours}h", _period.TotalHours);

            // Chạy ngay lần đầu khi khởi động
            await DoWorkAsync(stoppingToken);

            // Sau đó lặp theo _period
            using var timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested
                   && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }

            _logger.LogInformation("[ESC WORKER] Stopped");
        }

        private async Task DoWorkAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("[ESC WORKER] Running at {Time}", DateTimeOffset.Now);

                // Tạo scope mới cho mỗi lần chạy (vì ILeaveEscalationService là Scoped)
                await using var scope = _serviceProvider.CreateAsyncScope();
                var escalationService = scope.ServiceProvider
                    .GetRequiredService<ILeaveEscalationService>();

                await escalationService.ProcessAutoEscalationAsync(ct);

                _logger.LogInformation("[ESC WORKER] Completed at {Time}", DateTimeOffset.Now);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[ESC WORKER] Cancelled");
            }
            catch (Exception ex)
            {
                // Không throw — để vòng lặp tiếp tục chạy lần sau
                _logger.LogError(ex, "[ESC WORKER] Error during execution");
            }
        }
    }
}
