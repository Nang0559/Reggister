using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Shared.Handlers
{
    public interface IHttpClientWithAuth
    {
        Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default);
        Task<ApiResponse<T>> PostAsync<T>(string url, object data, CancellationToken ct = default);
        Task<ApiResponse<T>> PutAsync<T>(string url, object data, CancellationToken ct = default);
        Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default);
        Task<ApiResponse<T>> PatchAsync<T>(string url, object data, CancellationToken ct = default);
        Task<ApiResponse<PaginationResult<T>>> GetPagedAsync<T>(string url, CancellationToken ct = default);
        Task<ApiResponse<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content, CancellationToken ct = default);
        Task<ApiResponse<byte[]>> GetFileAsync(string url, CancellationToken ct = default);
        Task<ApiResponse<byte[]>> PostFileAsync(string url, object data, CancellationToken ct = default);
    }
}
