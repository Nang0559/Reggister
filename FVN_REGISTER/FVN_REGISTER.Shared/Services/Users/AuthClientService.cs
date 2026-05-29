using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;


namespace FVN_REGISTER.Shared.Services.Users
{
    public class AuthClientService : IAuthClientService
    {
        private readonly HttpClient _publicHttp;
        private readonly IHttpClientWithAuth _authHttp;
        private readonly ITokenStorage _tokenStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ICurrentUserClientService _currentUserService;
        private readonly ILogger<AuthClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public AuthClientService(
            HttpClient publicHttp,
            IHttpClientWithAuth authHttp,
            ITokenStorage tokenStorage,
            AuthenticationStateProvider authStateProvider,
            ICurrentUserClientService currentUserService,
            ILogger<AuthClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _publicHttp = publicHttp;
            _authHttp = authHttp;
            _tokenStorage = tokenStorage;
            _authStateProvider = authStateProvider;
            _currentUserService = currentUserService;
            _logger = logger;
            _options = options;
        }

        // ================= LOGIN =================
        public async Task<ApiResponse<UserSessionDto>> Login(
            string username,
            string password,
            CancellationToken ct = default)
        {
            try
            {
                var response = await _publicHttp.PostAsJsonAsync("api/auth/login", new
                {
                    UserName = username,
                    Password = password
                }, ct);

                var result = await response.Content
                    .ReadFromJsonAsync<ApiResponse<UserSessionDto>>(cancellationToken: ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarnIf(Debug,
                        "Login HTTP failed: {Status}",
                        response.StatusCode);
                }

                if (result?.IsSuccess == true && result.Data != null)
                {
                    var token = result.Data.Token!;

                    await _tokenStorage.SetTokenAsync(token);

                    if (_authStateProvider is CustomAuthStateProvider cp)
                        cp.NotifyUserLogin(token);

                    _logger.LogInfoIf(Debug,
                        "Login success: {User}",
                        username);

                    return result;
                }

                _logger.LogWarnIf(Debug,
                    "Login failed: {User} | {Msg}",
                    username,
                    result?.Message);

                return result ?? ApiResponse<UserSessionDto>.Fail("Sai tài khoản hoặc mật khẩu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login exception: {User}", username);
                return ApiResponse<UserSessionDto>.Fail("Không thể kết nối server.");
            }
        }

        // ================= PROFILE =================
        public async Task<ApiResponse<UserSessionDto>> GetProfileAsync(CancellationToken ct = default)
        {
            try
            {
                return await _authHttp.GetAsync<UserSessionDto>("api/auth/profile", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfile failed");
                return ApiResponse<UserSessionDto>.Fail("Lỗi lấy thông tin người dùng.");
            }
        }

        // ================= CHANGE PASSWORD =================
        public async Task<ApiResponse<object>> ChangePassword(
            string currentPassword,
            string newPassword,
            CancellationToken ct = default)
        {
            try
            {
                return await _authHttp.PostAsync<object>("api/auth/change-password", new
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePassword failed");
                return ApiResponse<object>.Fail("Lỗi đổi mật khẩu.");
            }
        }

        // ================= UPDATE PROFILE =================
        public async Task<ApiResponse<object>> UpdateProfileAsync(
            string email,
            string? avatarUrl,
            CancellationToken ct = default)
        {
            try
            {
                return await _authHttp.PutAsync<object>("api/auth/profile-update", new
                {
                    Email = email,
                    AvatarUrl = avatarUrl
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfile failed");
                return ApiResponse<object>.Fail("Lỗi cập nhật thông tin.");
            }
        }

        // ================= LOGOUT =================
        public async Task Logout(CancellationToken ct = default)
        {
            try
            {
                await _authHttp.PostAsync<object>("api/auth/logout", new { }, ct);

                _logger.LogInfoIf(Debug, "Logout API called");
            }
            catch (Exception ex)
            {
                _logger.LogWarnIf(Debug, "Logout API failed (ignored)");
                _logger.LogError(ex, "Logout API exception");
            }
            finally
            {
                await _tokenStorage.RemoveTokenAsync();
                _currentUserService.ClearUser();

                if (_authStateProvider is CustomAuthStateProvider cp)
                    cp.NotifyUserLogout();

                _logger.LogInfoIf(Debug, "Client logout completed");
            }
        }
    }
}
