using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Logging;


namespace FVN_REGISTER.Shared.Services.OTs
{
    public class DeptClientService : IDeptClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<DeptClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        // Cache đơn giản trong memory — reset khi circuit reset
        private List<DeptOption>? _cachedDepts;
        private DateTime _cacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        private bool Debug => _options.CurrentValue.Enabled;

        public DeptClientService(
            IHttpClientWithAuth http,
            ILogger<DeptClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public async Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[DEPT_CLIENT] GetDepartments");
                return await _http.GetAsync<List<DeptOption>>("api/Common/departments", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DEPT_CLIENT] GetDepartments lỗi");
                return ApiResponse<List<DeptOption>>.Fail("Lỗi tải danh sách phòng ban.");
            }
        }

        public async Task<List<DeptOption>> GetDepartmentsCachedAsync(
            CancellationToken ct = default)
        {
            // Trả cache nếu còn hạn
            if (_cachedDepts != null && DateTime.Now - _cacheTime < CacheDuration)
            {
                _logger.LogDebugIf(Debug, "[DEPT_CLIENT] Dùng cache departments");
                return _cachedDepts;
            }

            var result = await GetDepartmentsAsync(ct);

            if (result.IsSuccess && result.Data != null)
            {
                _cachedDepts = result.Data;
                _cacheTime = DateTime.Now;
            }

            return _cachedDepts ?? new List<DeptOption>();
        }
    }
}
