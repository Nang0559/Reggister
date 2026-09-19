using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Application.Logging;

public static class LoggerExtensions
{
    public static void LogDebugIf(
        this ILogger logger,
        bool enabled,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogDebug(message, args);
    }

    public static void LogInfoIf(
        this ILogger logger,
        bool enabled,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogInformation(message, args);
    }

    public static void LogWarnIf(
        this ILogger logger,
        bool enabled,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogWarning(message, args);
    }

    public static void LogErrorIf(
        this ILogger logger,
        bool enabled,
        Exception exception,
        string message,
        params object?[] args)
    {
        if (enabled)
            logger.LogError(exception, message, args);
    }
}