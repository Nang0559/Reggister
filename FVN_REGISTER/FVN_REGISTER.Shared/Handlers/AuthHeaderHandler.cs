using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;


namespace FVN_REGISTER.Shared.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly ITokenStorage _tokenStorage;
        private readonly ILogger<AuthHeaderHandler> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        public AuthHeaderHandler(
            CustomAuthStateProvider authStateProvider,
            ITokenStorage tokenStorage,
            ILogger<AuthHeaderHandler> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _authStateProvider = authStateProvider;
            _tokenStorage = tokenStorage;
            _logger = logger;
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
     HttpRequestMessage request,
     CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? "";
            var debug = _options.CurrentValue.Enabled;

            // 1. Bỏ qua luồng Login
            if (path.Contains("api/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            // 2. THAY ĐỔI QUAN TRỌNG: Lấy token trực tiếp từ Storage thay vì Provider
            // Vì Storage của chúng ta đã có biến static _staticCachedToken nên lấy cực nhanh và chính xác
            var token = await _tokenStorage.GetTokenAsync();

            // Nếu vẫn muốn ưu tiên RAM của Provider thì dùng logic này:
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _authStateProvider.CurrentToken;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                // 3. Gán Header (Lưu ý Trim token để tránh lỗi format)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim('"').Trim());

                if (debug) _logger.LogInformation("[AUTH] Token attached | Path={Path}", path);
            }
            else
            {
                // Log warn này sẽ không còn xuất hiện nữa vì đã có Static Cache ở Storage gánh
                _logger.LogWarning("[AUTH] Missing token | Path={Path}", path);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
