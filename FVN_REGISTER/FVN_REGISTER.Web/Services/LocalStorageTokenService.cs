

namespace FVN_REGISTER.Web.Services
{

    using global::FVN_REGISTER.Contract.Utils;
    using global::FVN_REGISTER.Shared.Utils;
    using Microsoft.JSInterop;
    using System;
    using System.Threading.Tasks;

    namespace FVN_REGISTER.Web.Services
    {
        public class LocalStorageTokenService : ITokenStorage
        {
            private readonly IJSRuntime _jsRuntime;
            private readonly ILogger<LocalStorageTokenService> _logger;
            private readonly bool _debug;

            // 🔥 CHỐT CHẶN: Dùng static để tất cả các Instance (Handler, AuthProvider, v.v.) 
            // đều nhìn thấy cùng một giá trị Token trong RAM của Server Circuit.
            private static string? _staticCachedToken;
            private bool _jsReady = false;

            public void MarkJsReady()
            {
                _jsReady = true;
                if (_debug) _logger.LogInformation("[TokenService:{Id}] JS marked as ready", InstanceId);
            }
            public string InstanceId { get; } = Guid.NewGuid().ToString()[..8];

            public LocalStorageTokenService(
                IJSRuntime jsRuntime,
                ILogger<LocalStorageTokenService> logger,
                IConfiguration config)
            {
                _jsRuntime = jsRuntime;
                _logger = logger;
                _debug = config.GetValue<bool>("AuthDebug:Enabled") || true; // Ép bật debug để dễ theo dõi

                if (_debug) _logger.LogInformation("[TokenService:{Id}] Instance Initialized", InstanceId);
            }

            // ================= GET =================
            public async Task<string?> GetTokenAsync()
            {
                // 1. Ưu tiên số 1: Lấy ngay từ RAM tĩnh (Cực nhanh, không lỗi JS)
                if (!string.IsNullOrEmpty(_staticCachedToken))
                {
                    if (_debug) _logger.LogInformation("[TokenService:{Id}] Token retrieved from STATIC CACHE", InstanceId);
                    return _staticCachedToken;
                }

                try
                {
                    // 2. Ưu tiên số 2: Nếu RAM trống (do F5 trang), lấy từ trình duyệt
                    var token = await _jsRuntime.InvokeAsync<string?>(
                        "localStorage.getItem",
                        AuthConstants.TokenKey);

                    _staticCachedToken = token?.Trim('"');
                    return _staticCachedToken;
                }
                catch (Exception ex)
                {
                    if (_debug) _logger.LogWarning("[TokenService:{Id}] JS not ready for GET - Using fallback cache", InstanceId);
                    return _staticCachedToken;
                }
            }

            // ================= SET =================
            public async Task SetTokenAsync(string token)
            {
                if (string.IsNullOrWhiteSpace(token)) return;

                // 1. Nạp "đạn" vào RAM tĩnh ngay lập tức
                _staticCachedToken = token.Trim('"').Trim();

                try
                {
                    // 2. Đẩy xuống trình duyệt để lưu lâu dài
                    await _jsRuntime.InvokeVoidAsync(
                        "localStorage.setItem",
                        AuthConstants.TokenKey,
                        _staticCachedToken);

                    if (_debug) _logger.LogInformation("[TokenService:{Id}] Token SAVED to static cache and LocalStorage", InstanceId);
                }
                catch (Exception ex)
                {
                    // Sử dụng ?. để tránh lỗi ArgumentNullException nếu logger chưa kịp init
                    _logger?.LogError(ex, "[TokenService:{Id}] SetTokenAsync error", InstanceId);
                }
            }

            // ================= REMOVE =================
            public async Task RemoveTokenAsync()
            {
                _staticCachedToken = null;
                try
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthConstants.TokenKey);
                    if (_debug) _logger.LogInformation("[TokenService:{Id}] Token REMOVED", InstanceId);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("[TokenService:{Id}] RemoveTokenAsync error", InstanceId);
                }
            }

        }
    }
}
