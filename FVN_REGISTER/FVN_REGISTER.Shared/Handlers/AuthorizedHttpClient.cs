using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FVN_REGISTER.Shared.Handlers
{
    public class AuthorizedHttpClient : IHttpClientWithAuth
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthorizedHttpClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthorizedHttpClient(
            HttpClient httpClient,
            ILogger<AuthorizedHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _logger.LogDebug(
                "[HTTP] Base Address: {Base}",
                _httpClient.BaseAddress);
        }

        private async Task<ApiResponse<T>> SendAsync<T>(
            Func<CancellationToken, Task<HttpResponseMessage>> sendFunc,
            string url,
            CancellationToken ct)
        {
            try
            {
                _logger.LogDebug("[HTTP] -> {Url}", url);

                using var response = await sendFunc(ct);

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
                    {
                        return errorResponse;
                    }
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
            {
                return apiResult;
            }

            var raw = TryDeserialize<T>(content);
            if (raw != null)
            {
                return ApiResponse<T>.Ok(raw);
            }

            return ApiResponse<T>.Fail("Invalid response format.");
        }

        private static T? TryDeserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

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
                c => _httpClient.GetAsync(url, c),
                url,
                ct);

        public Task<ApiResponse<T>> PostAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(
                c => _httpClient.PostAsJsonAsync(url, data, c),
                url,
                ct);

        public Task<ApiResponse<T>> PutAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(
                c => _httpClient.PutAsJsonAsync(url, data, c),
                url,
                ct);

        public Task<ApiResponse<T>> DeleteAsync<T>(
            string url,
            CancellationToken ct = default)
            => SendAsync<T>(
                c => _httpClient.DeleteAsync(url, c),
                url,
                ct);

        public Task<ApiResponse<T>> PatchAsync<T>(
            string url,
            object data,
            CancellationToken ct = default)
            => SendAsync<T>(
                c => _httpClient.PatchAsJsonAsync(url, data, c),
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
                using var response = await _httpClient.GetAsync(url, ct);

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
                c => _httpClient.PostAsync(url, content, c),
                url,
                ct);
    }
}
