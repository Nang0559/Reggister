using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Application.Logging;

public static class LoggerExtensions
{
    public static void LogDebugIf<T>(
        this ILogger<T> logger,
        bool enabled,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogDebug(message, args);
    }

    public static void LogInfoIf<T>(
        this ILogger<T> logger,
        bool enabled,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogInformation(message, args);
    }

    public static void LogWarnIf<T>(
        this ILogger<T> logger,
        bool enabled,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogWarning(message, args);
    }
}
