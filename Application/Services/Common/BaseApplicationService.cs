using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Application.Services.Common
{
    public abstract class BaseApplicationService<T> : BaseService<T>
    {
        protected BaseApplicationService(
            ILogger<T> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
        }

        protected ServiceResult<TResult> InternalError<TResult>(
            Exception ex, string customMsg = "Lỗi hệ thống")
        {
            Logger.LogError(ex, "[{Component}] {Message}", ComponentName, ex.Message);
            var finalMsg = Debug ? $"{customMsg}: {ex.Message}" : customMsg;
            return ServiceResult<TResult>.Fail(finalMsg);
        }
    }
}
