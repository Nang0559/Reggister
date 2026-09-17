using FVN_REGISTER.Shared.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace FVN_REGISTER.Shared.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly ITokenStorage _tokenStorage;
        private readonly ILogger<AuthHeaderHandler> _logger;

        public AuthHeaderHandler(
            CustomAuthStateProvider authStateProvider,
            ITokenStorage tokenStorage,
            ILogger<AuthHeaderHandler> logger)
        {
            _authStateProvider = authStateProvider;
            _tokenStorage = tokenStorage;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (path.Contains("api/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var token = await _tokenStorage.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                token = _authStateProvider.CurrentToken;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token.Trim('"').Trim());

                _logger.LogDebug(
                    "[AUTH] Token attached | Path={Path}",
                    path);
            }
            else
            {
                _logger.LogWarning(
                    "[AUTH] Missing token | Path={Path}",
                    path);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
