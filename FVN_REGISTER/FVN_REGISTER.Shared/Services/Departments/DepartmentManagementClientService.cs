using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Departments
{
    public sealed class DepartmentManagementClientService : IDepartmentManagementClientService
    {
        private const string BaseUrl = "api/admin/departments";
        private readonly IHttpClientWithAuth _http;

        public DepartmentManagementClientService(IHttpClientWithAuth http)
        {
            _http = http;
        }

        public Task<ApiResponse<List<DepartmentDto>>> GetTreeAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<DepartmentDto>>($"{BaseUrl}/tree", ct);

        public Task<ApiResponse<List<DepartmentDto>>> GetListAsync(
            bool? isActive = null,
            CancellationToken ct = default)
        {
            var url = isActive.HasValue
                ? $"{BaseUrl}?isActive={isActive.Value}"
                : BaseUrl;
            return _http.GetAsync<List<DepartmentDto>>(url, ct);
        }

        public Task<ApiResponse<DepartmentDto>> GetByIdAsync(
            int id,
            CancellationToken ct = default)
            => _http.GetAsync<DepartmentDto>($"{BaseUrl}/{id}", ct);

        public Task<ApiResponse<object>> CreateAsync(
            DepartmentUpsertDto model,
            CancellationToken ct = default)
            => _http.PostAsync<object>(BaseUrl, model, ct);

        public Task<ApiResponse<object>> UpdateAsync(
            int id,
            DepartmentUpsertDto model,
            CancellationToken ct = default)
            => _http.PutAsync<object>($"{BaseUrl}/{id}", model, ct);

        public Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
            => _http.PatchAsync<object>($"{BaseUrl}/{id}/toggle", new { }, ct);

        public Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default)
            => _http.DeleteAsync<object>($"{BaseUrl}/{id}", ct);
    }
}
