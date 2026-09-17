using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;


namespace FVN_REGISTER.Shared.Handlers
{
    public class AuthorizedHttpClient : IHttpClientWithAuth
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthorizedHttpClient> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthorizedHttpClient(
            HttpClient httpClient,
            ILogger<AuthorizedHttpClient> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options;

            _logger.LogInfoIf(Debug,
                "[HTTP] Base Address: {Base}",
                _httpClient.BaseAddress);
        }

        // ================= CORE =================
        private async Task<ApiResponse<T>> SendAsync<T>(
            Func<CancellationToken, Task<HttpResponseMessage>> sendFunc,
            string url,
            CancellationToken ct)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[HTTP] → {Url}", url);

                var response = await sendFunc(ct);

                _logger.LogDebugIf(Debug,
                    "[HTTP] ← {Url} | Status={Status}",
                    url,
                    (int)response.StatusCode);

                return await HandleResponseAsync<T>(response, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HTTP] Connection error: {Url}", url);
                return ApiResponse<T>.Fail("Không thể kết nối đến server.");
            }
        }

        // ================= RESPONSE HANDLE =================
        private async Task<ApiResponse<T>> HandleResponseAsync<T>(
    HttpResponseMessage response,
    string url)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var isAuthError = response.StatusCode == HttpStatusCode.Unauthorized;

                _logger.LogWarnIf(Debug,
                    "[HTTP] Error {Url} | Status={Status}",
                    url,
                    (int)response.StatusCode);

                // 🔥 FIX: Thử parse ApiResponse để lấy message từ server
                var errorResponse = TryDeserialize<ApiResponse<T>>(content);
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    errorResponse.IsSuccess = false;
                    errorResponse.StatusCode = (int)response.StatusCode;
                    errorResponse.IsUnauthorized = isAuthError;
                    return errorResponse;
                }

                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Message = content, // fallback raw content
                    IsUnauthorized = isAuthError
                };
            }

            // 1. Try ApiResponse<T>
            var apiResult = TryDeserialize<ApiResponse<T>>(content);
            if (apiResult != null)
            {
                _logger.LogDebugIf(Debug, "[HTTP] Parsed ApiResponse<T> OK");
                return apiResult;
            }

            // 2. Try raw T
            var raw = TryDeserialize<T>(content);
            if (raw != null)
            {
                _logger.LogDebugIf(Debug, "[HTTP] Parsed raw T OK");
                return ApiResponse<T>.Ok(raw);
            }

            _logger.LogWarnIf(Debug, "[HTTP] Parse failed: {Url}", url);
            return ApiResponse<T>.Fail("Invalid response format.");
        }
      
        private static T? TryDeserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        // ================= METHODS =================
        public Task<ApiResponse<T>> PatchAsync<T>(string url, object data, CancellationToken ct = default)
        => SendAsync<T>(c => _httpClient.PatchAsJsonAsync(url, data, c), url, ct);
        public Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default)
            => SendAsync<T>(c => _httpClient.GetAsync(url, c), url, ct);

        public Task<ApiResponse<T>> PostAsync<T>(string url, object data, CancellationToken ct = default)
            => SendAsync<T>(c => _httpClient.PostAsJsonAsync(url, data, c), url, ct);

        public Task<ApiResponse<T>> PutAsync<T>(string url, object data, CancellationToken ct = default)
            => SendAsync<T>(c => _httpClient.PutAsJsonAsync(url, data, c), url, ct);

        public Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
            => SendAsync<T>(c => _httpClient.DeleteAsync(url, c), url, ct);

        public Task<ApiResponse<PaginationResult<T>>> GetPagedAsync<T>(string url, CancellationToken ct = default)
            => GetAsync<PaginationResult<T>>(url, ct);

        public async Task<ApiResponse<byte[]>> GetFileAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var res = await _httpClient.GetAsync(url, ct);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarnIf(Debug,
                        "[HTTP] File download failed: {Url}",
                        url);

                    return ApiResponse<byte[]>.Fail("Download failed");
                }

                var bytes = await res.Content.ReadAsByteArrayAsync(ct);

                _logger.LogDebugIf(Debug,
                    "[HTTP] File downloaded OK: {Url}",
                    url);

                return ApiResponse<byte[]>.Ok(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HTTP] File download error: {Url}", url);
                return ApiResponse<byte[]>.Fail("Download error");
            }
        }

        public Task<ApiResponse<T>> PostMultipartAsync<T>(
            string url,
            MultipartFormDataContent content,
            CancellationToken ct = default)
            => SendAsync<T>(c => _httpClient.PostAsync(url, content, c), url, ct);
    }
}
