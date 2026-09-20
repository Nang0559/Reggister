using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Contract.Requests.Auths;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FVN_REGISTER.Shared.Services.Users
{
    public sealed class AuthClientService : IAuthClientService
    {
        private readonly HttpClient _publicHttp;
        private readonly IHttpClientWithAuth _authHttp;
        private readonly ITokenStorage _tokenStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ICurrentUserClientService _currentUserService;
        private readonly ILogger<AuthClientService> _logger;

        public AuthClientService(HttpClient publicHttp, IHttpClientWithAuth authHttp, ITokenStorage tokenStorage, AuthenticationStateProvider authStateProvider, ICurrentUserClientService currentUserService, ILogger<AuthClientService> logger)
        {
            _publicHttp = publicHttp; _authHttp = authHttp; _tokenStorage = tokenStorage; _authStateProvider = authStateProvider; _currentUserService = currentUserService; _logger = logger;
        }

        public async Task<ApiResponse<AuthResultDto>> Login(string username, string password, CancellationToken ct = default)
        {
            try
            {
                var response = await _publicHttp.PostAsJsonAsync("api/auth/login", new LoginRequestDto { UserName = username, Password = password }, ct);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>(cancellationToken: ct);
                if (!response.IsSuccessStatusCode) _logger.LogWarning("Login HTTP failed: {Status}", response.StatusCode);
                if (result?.IsSuccess == true && result.Data != null && !string.IsNullOrWhiteSpace(result.Data.Token))
                {
                    await _tokenStorage.SetTokenAsync(result.Data.Token);
                    if (!string.IsNullOrWhiteSpace(result.Data.RefreshToken)) await _tokenStorage.SetRefreshTokenAsync(result.Data.RefreshToken);
                    if (_authStateProvider is CustomAuthStateProvider provider) provider.NotifyUserLogin(result.Data.Token);
                    return result;
                }
                return result ?? ApiResponse<AuthResultDto>.Fail("Sai tài khoản hoặc mật khẩu.");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogError(ex, "Login exception: {User}", username); return ApiResponse<AuthResultDto>.Fail("Không thể kết nối server."); }
        }

        public async Task<ApiResponse<UserIdentityDto>> GetProfileAsync(CancellationToken ct = default)
        {
            try { return await _authHttp.GetAsync<UserIdentityDto>("api/auth/profile", ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogError(ex, "GetProfile failed"); return ApiResponse<UserIdentityDto>.Fail("Lỗi lấy thông tin người dùng."); }
        }

        public async Task<ApiResponse<object>> ChangePassword(string currentPassword, string newPassword, CancellationToken ct = default)
        {
            try { return await _authHttp.PostAsync<object>("api/auth/change-password", new { CurrentPassword = currentPassword, NewPassword = newPassword }, ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogError(ex, "ChangePassword failed"); return ApiResponse<object>.Fail("Lỗi đổi mật khẩu."); }
        }

        public async Task<ApiResponse<object>> UpdateProfileAsync(string email, string? avatarUrl, CancellationToken ct = default)
        {
            try
            {
                return await _authHttp.PutAsync<object>("api/auth/profile-update", new UpdateProfileCommandDto { Email = email, AvatarUrl = avatarUrl }, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogError(ex, "UpdateProfile failed"); return ApiResponse<object>.Fail("Lỗi cập nhật thông tin."); }
        }

        public async Task<ApiResponse<List<SessionDto>>> GetSessionsAsync(CancellationToken ct = default)
        {
            try { return await _authHttp.GetAsync<List<SessionDto>>("api/auth/sessions", ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogError(ex, "GetSessions failed"); return ApiResponse<List<SessionDto>>.Fail("Không tải được danh sách thiết bị."); }
        }

        public async Task<ApiResponse<object>> RevokeSessionAsync(int sessionId, CancellationToken ct = default)
        {
            try { return await _authHttp.DeleteAsync<object>($"api/auth/sessions/{sessionId}", ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogError(ex, "RevokeSession failed: {SessionId}", sessionId); return ApiResponse<object>.Fail("Không thể đăng xuất thiết bị."); }
        }

        public async Task Logout(CancellationToken ct = default)
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            try
            {
                await _authHttp.PostAsync<object>("api/auth/logout", new LogoutRequestDto { RefreshToken = refreshToken }, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogWarning(ex, "Logout API failed (ignored)"); }
            finally
            {
                await _tokenStorage.ClearAsync();
                _currentUserService.ClearUser();
                if (_authStateProvider is CustomAuthStateProvider provider) provider.NotifyUserLogout();
            }
        }
    }
}
