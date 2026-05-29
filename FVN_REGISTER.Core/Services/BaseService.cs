using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Core.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Core.Services
{
    public abstract class BaseService<T>
    {
        protected readonly ILogger<T> Logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        // Giúp log tự động lấy tên Class (ví dụ: DashboardService)
        protected string ComponentName => typeof(T).Name;

        // Trạng thái Debug lấy từ cấu hình hệ thống
        protected bool Debug => _options.CurrentValue.Enabled;

        protected BaseService(
            ILogger<T> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            Logger = logger;
            _options = options;
        }

        // Bạn có thể thêm một hàm trợ giúp để trả về Result lỗi nhanh
        protected ServiceResult<TResult> InternalError<TResult>(Exception ex, string customMsg = "Lỗi hệ thống")
        {
            // Luôn log lỗi chi tiết ở server
            Logger.LogError(ex, "[{Component}] {Message}", ComponentName, ex.Message);

            // Trả về message chi tiết nếu đang ở chế độ Debug, ngược lại trả về thông báo chung
            var finalMsg = Debug ? $"{customMsg}: {ex.Message}" : customMsg;
            return ServiceResult<TResult>.Fail(finalMsg);
        }
    }
}
