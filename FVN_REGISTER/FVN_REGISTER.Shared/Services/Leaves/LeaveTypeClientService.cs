using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public sealed class LeaveTypeClientService : ILeaveTypeClientService
    {
        private const string BaseUrl = "api/LeaveTypeManagement";
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<LeaveTypeClientService> _logger;

        public LeaveTypeClientService(
            IHttpClientWithAuth http,
            ILogger<LeaveTypeClientService> logger)
        {
            _http = http;
            _logger = logger;
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

                return await _http.GetAsync<List<LeaveTypeDto>>(url, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] GetList error");
                return ApiResponse<List<LeaveTypeDto>>.Fail("Không thể tải danh sách loại nghỉ.");
            }
        }

        public async Task<ApiResponse<object>> CreateAsync(
            LeaveTypeUpsertDto model,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.PostAsync<object>(BaseUrl, model, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Create error");
                return ApiResponse<object>.Fail("Không thể tạo loại nghỉ.");
            }
        }

        public async Task<ApiResponse<object>> UpdateAsync(
            int id,
            LeaveTypeUpsertDto model,
            CancellationToken ct = default)
        {
            try
            {
                model.Id = id;
                return await _http.PutAsync<object>($"{BaseUrl}/{id}", model, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Update error");
                return ApiResponse<object>.Fail("Không thể cập nhật loại nghỉ.");
            }
        }

        public async Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.PatchAsync<object>($"{BaseUrl}/{id}/toggle", new { }, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Toggle error");
                return ApiResponse<object>.Fail("Không thể thay đổi trạng thái loại nghỉ.");
            }
        }

        public async Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.DeleteAsync<object>($"{BaseUrl}/{id}", ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_TYPE_CLIENT] Delete error");
                return ApiResponse<object>.Fail("Không thể xóa loại nghỉ.");
            }
        }
    }
}
