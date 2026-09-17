using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



// Application/Common/BaseService.cs — gộp về đây, xoá bản ở Core
namespace FVN_REGISTER.Application.Services.Common
{
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
        // <summary>
        /// Log lỗi chi tiết ở server, trả về ServiceResult&lt;TResult&gt; thất bại.
        /// Ở môi trường Debug, message trả về client sẽ kèm chi tiết exception để dev dễ trace;
        /// ở Production chỉ trả customMsg chung chung, tránh lộ thông tin nội bộ.
        /// </summary>
        protected ServiceResult<TResult> InternalError<TResult>(
            Exception ex, string customMsg = "Lỗi hệ thống")
        {
            Logger.LogError(ex, "[{Component}] {Message}", ComponentName, ex.Message);

            var finalMsg = Debug ? $"{customMsg}: {ex.Message}" : customMsg;
            return ServiceResult<TResult>.Fail(finalMsg);
        }

        /// <summary>
        /// Overload không generic, dùng cho các method trả về ServiceResult (không có Data).
        /// </summary>
        protected ServiceResult InternalError(
            Exception ex, string customMsg = "Lỗi hệ thống")
        {
            Logger.LogError(ex, "[{Component}] {Message}", ComponentName, ex.Message);

            var finalMsg = Debug ? $"{customMsg}: {ex.Message}" : customMsg;
            return ServiceResult.Fail(finalMsg);
        }
    }
}

