using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

public static class JobRetryPolicy
{
    public static ResiliencePipeline Create(string operationName, ILogger logger)
        => new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                Name = operationName,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "[JOB-RETRY] {Operation} retry {Attempt} after {Delay}.",
                        operationName,
                        args.AttemptNumber + 1,
                        args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
}
