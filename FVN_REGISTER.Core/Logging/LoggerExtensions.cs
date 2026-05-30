using Microsoft.Extensions.Logging;
using System;


namespace FVN_REGISTER.Core.Logging
{
    public static class LoggerExtensions
    {
        // ===== DEBUG =====
        public static void LogDebugIf(
            this ILogger logger,
            bool enabled,
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(message, args);
        }

        public static void LogDebugIf<T>(
            this ILogger<T> logger,
            bool enabled,
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(message, args);
        }

        // ===== INFO =====
        public static void LogInfoIf(
            this ILogger logger,
            bool enabled,
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(message, args);
        }

        public static void LogInfoIf<T>(
            this ILogger<T> logger,
            bool enabled,
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(message, args);
        }

        // ===== WARN =====
        public static void LogWarnIf(
            this ILogger logger,
            bool enabled,
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning(message, args);
        }

        public static void LogWarnIf<T>(
            this ILogger<T> logger,
            bool enabled,
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning(message, args);
        }
        // ===== ERROR =====
        public static void LogErrorIf(
            this ILogger logger,
            bool enabled,
            Exception exception, // Thêm tham số nhận exception
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Error))
                logger.LogError(exception, message, args);
        }

        public static void LogErrorIf<T>(
            this ILogger<T> logger,
            bool enabled,
            Exception exception, // Thêm tham số nhận exception
            string message,
            params object[] args)
        {
            if (enabled && logger.IsEnabled(LogLevel.Error))
                logger.LogError(exception, message, args);
        }
    }
}
