using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Shared.Utils;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FVN_REGISTER.Shared.Handlers
{
    public class AuthorizedHttpClient : IHttpClientWithAuth
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthorizedHttpClient> _logger;
        private readonly ITokenStorage _tokenStorage;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthorizedHttpClient(
            HttpClient httpClient,
            ILogger<AuthorizedHttpClient> logger,
            ITokenStorage tokenStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenStorage = tokenStorage;

            _logger.LogDebug(
                "[HTTP] Base Address: {Base}",
                _httpClient.BaseAddress);
        }

        private async Task AttachTokenAsync(HttpRequestMessage request)
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token.Trim('"').Trim());

                _logger.LogDebug(
                    "[AUTH] Token attached | Path={Path}",
                    request.RequestUri?.ToString());
            }
            else
            {
                _logger.LogWarning(
                    "[AUTH] Missing token | Path={Path}",
                    request.RequestUri?.AbsolutePath);
            }
        }

        private async Task<ApiResponse<T>> SendAsync<T>(
            Func<HttpRequestMessage> requestFactory,
            string url,
            CancellationToken ct)
        {
            try
            {
                using var request = requestFactory();
                await AttachTokenAsync(request);

                _logger.LogDebug("[HTTP] -> {Url}", url);

                using var response = await _httpClient.SendAsync(request, ct);

                _logger.LogDebug(
                    "[HTTP] <- {Url} | Status={Status}",
                    url,
                    (int)response.StatusCode);

                return await HandleResponseAsync<T>(response, url, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[HTTP] Connection error: {Url}",
                    url);

                return ApiResponse<T>.Fail("Không thể kết nối đến server.");
            }
        }

        private async Task<ApiResponse<T>> HandleResponseAsync<T>(
            HttpResponseMessage response,
            string url,
            CancellationToken ct)
        {
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                var isUnauthorized = response.StatusCode == HttpStatusCode.Unauthorized;
                var errorResponse = TryDeserialize<ApiResponse<T>>(content);

                if (errorResponse != null)
                {
                    errorResponse.IsSuccess = false;
                    errorResponse.StatusCode = (int)response.StatusCode;
                    errorResponse.IsUnauthorized = isUnauthorized;

                    if (!string.IsNullOrWhiteSpace(errorResponse.Message))
                        return errorResponse;
                }

                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Message = string.IsNullOrWhiteSpace(content)
                        ? "Server request failed."
                        : content,
                    IsUnauthorized = isUnauthorized
                };
            }

            var apiResult = TryDeserialize<ApiResponse<T>>(content);
            if (apiResult != null)
                return apiResult;

            var raw = TryDeserialize<T>(content);
            if (raw != null)
                return ApiResponse<T>.Ok(raw);

            return ApiResponse<T>.Fail("Invalid response format.");
        }

        private static T? TryDeserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        public Task<ApiResponse<T>> GetAsync<T>(
            string url,
            CancellationToken ct = default)
            => SendAsync<T>(
                () => new HttpRequestMessage(HttpMethod.Get, url),
                url,
                ct);

        public async Task<ApiResponse<byte[]>> PostFileAsync(
            string url,
            object data,
            CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(data)
                };
                await AttachTokenAsync(request);

                using var response = await _httpClient.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync(ct);
                    return ApiResponse<byte[]>.Fail(
                        string.IsNullOrWhiteSpace(message) ? "Export failed" : message);
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                return ApiResponse<byte[]>.Ok(bytes);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HTTP] POST file download error: {Url}", url);
                return ApiResponse<byte[]>.Fail("Download error");
            }
        }

        public Task<ApiResponse<T>> PostAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(
                () => new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(data)
                },
                url,
                ct);

        public Task<ApiResponse<T>> PutAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(
                () => new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = JsonContent.Create(data)
                },
                url,
                ct);

        public Task<ApiResponse<T>> DeleteAsync<T>(
            string url,
            CancellationToken ct = default)
            => SendAsync<T>(
                () => new HttpRequestMessage(HttpMethod.Delete, url),
                url,
                ct);

        public Task<ApiResponse<T>> PatchAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(
                () => new HttpRequestMessage(HttpMethod.Patch, url)
                {
                    Content = JsonContent.Create(data)
                },
                url,
                ct);

        public Task<ApiResponse<PaginationResult<T>>> GetPagedAsync<T>(
            string url,
            CancellationToken ct = default)
            => GetAsync<PaginationResult<T>>(url, ct);

        public async Task<ApiResponse<byte[]>> GetFileAsync(
            string url,
            CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                await AttachTokenAsync(request);

                using var response = await _httpClient.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "[HTTP] File download failed: {Url} | Status={Status}",
                        url,
                        (int)response.StatusCode);

                    return ApiResponse<byte[]>.Fail("Download failed");
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                return ApiResponse<byte[]>.Ok(bytes);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[HTTP] File download error: {Url}",
                    url);

                return ApiResponse<byte[]>.Fail("Download error");
            }
        }

        public Task<ApiResponse<T>> PostMultipartAsync<T>(
            string url,
            MultipartFormDataContent content,
            CancellationToken ct = default)
            => SendAsync<T>(
                () => new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                },
                url,
                ct);
    }
}
