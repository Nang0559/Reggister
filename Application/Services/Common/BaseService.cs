using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Contract.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Application.Services.Common;

public abstract class BaseService<T>
{
    protected readonly ILogger<T> Logger;
    private readonly IOptionsMonitor<AuthDebugOptions> _options;

    protected string ComponentName => typeof(T).Name;
    protected bool Debug => _options.CurrentValue.Enabled;

    protected BaseService(
        ILogger<T> logger,
        IOptionsMonitor<AuthDebugOptions> options)
    {
        Logger = logger;
        _options = options;
    }

    protected ServiceResult<TResult> InternalError<TResult>(
        Exception ex, string customMsg = "Lỗi hệ thống")
    {
        Logger.LogError(ex, "[{Component}] {Message}", ComponentName, ex.Message);
        var finalMsg = Debug ? $"{customMsg}: {ex.Message}" : customMsg;
        return ServiceResult<TResult>.Fail(finalMsg);
    }

    protected ServiceResult InternalError(
        Exception ex, string customMsg = "Lỗi hệ thống")
    {
        Logger.LogError(ex, "[{Component}] {Message}", ComponentName, ex.Message);
        var finalMsg = Debug ? $"{customMsg}: {ex.Message}" : customMsg;
        return ServiceResult.Fail(finalMsg);
    }
}
