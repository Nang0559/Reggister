using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class LeaveTypeClientService : ILeaveTypeClientService
    {
        private const string BaseUrl = "api/LeaveTypeManagement";

        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<LeaveTypeClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public LeaveTypeClientService(
            IHttpClientWithAuth http,
            ILogger<LeaveTypeClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public async Task<ApiResponse<List<LeaveTypeDto>>> GetListAsync(
            bool? isCountedAsLeave = null,
            bool? isActive = null,
            CancellationToken ct = default)
        {
            try
            {
                var query = new List<string>();
                if (isCountedAsLeave.HasValue)
                    query.Add($"tinhPhep={isCountedAsLeave.Value}");
                if (isActive.HasValue)
                    query.Add($"isActive={isActive.Value}");

                var url = query.Count == 0
                    ? BaseUrl
                    : $"{BaseUrl}/filter?{string.Join("&", query)}";

                _logger.LogDebugIf(Debug, "[LEAVE_TYPE_CLIENT] GET {Url}", url);
                return await _http.GetAsync<List<LeaveTypeDto>>(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] GetList error");
                return ApiResponse<List<LeaveTypeDto>>.Fail("Không thể tải danh sách loại nghỉ.");
            }
        }

        public async Task<ApiResponse<LeaveTypeDto>> CreateAsync(
            LeaveTypeUpsertDto model,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.PostAsync<LeaveTypeDto>(BaseUrl, model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Create error");
                return ApiResponse<LeaveTypeDto>.Fail("Không thể tạo loại nghỉ.");
            }
        }

        public async Task<ApiResponse<LeaveTypeDto>> UpdateAsync(
            int id,
            LeaveTypeUpsertDto model,
            CancellationToken ct = default)
        {
            try
            {
                model.Id = id;
                return await _http.PutAsync<LeaveTypeDto>($"{BaseUrl}/{id}", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Update error");
                return ApiResponse<LeaveTypeDto>.Fail("Không thể cập nhật loại nghỉ.");
            }
        }

        public async Task<ApiResponse<bool>> ToggleAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _http.PatchAsync<bool>($"{BaseUrl}/{id}/toggle", new { }, ct);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Toggle error");
                return ApiResponse<bool>.Fail("Không thể thay đổi trạng thái loại nghỉ.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.DeleteAsync<bool>($"{BaseUrl}/{id}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Delete error");
                return ApiResponse<bool>.Fail("Không thể xóa loại nghỉ.");
            }
        }
    }
}
