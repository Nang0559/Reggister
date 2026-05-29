using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Shared.Utils.Helpers;


namespace FVN_REGISTER.Shared.Handlers
{
    public interface IHttpClientWithAuth
    {
     

        Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default);
        Task<ApiResponse<T>> PostAsync<T>(string url, object data, CancellationToken ct = default);
        Task<ApiResponse<T>> PutAsync<T>(string url, object data, CancellationToken ct = default);
        Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default);

        // Hỗ trợ phân trang đặc thù của FCC
        Task<ApiResponse<PaginationResult<T>>> GetPagedAsync<T>(string url, CancellationToken ct = default);

        // Hỗ trợ Upload/Download File
        Task<ApiResponse<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content, CancellationToken ct = default);
        Task<ApiResponse<byte[]>> GetFileAsync(string url, CancellationToken ct = default);
    }
}
