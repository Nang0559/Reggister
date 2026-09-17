using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Core.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using FVN_REGISTER.Contract.ViewModels.Leaves;


namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class LeaveTypeClientService : ILeaveTypeClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<LeaveTypeClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;
        private const string BaseUrl = "api/LeaveTypeManagement";

        public LeaveTypeClientService(
            IHttpClientWithAuth http,
            ILogger<LeaveTypeClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        // ── GetList (phẳng, tuỳ filter) ──────────────────────────
        public async Task<ApiResponse<List<LeaveTypeFlatViewModel>>> GetListAsync(
            bool? tinhPhep = null,
            bool? isActive = null,
            CancellationToken ct = default)
        {
            try
            {
                string url;
                if (tinhPhep.HasValue || isActive.HasValue)
                {
                    var qs = new List<string>();
                    if (tinhPhep.HasValue) qs.Add($"tinhPhep={tinhPhep.Value}");
                    if (isActive.HasValue) qs.Add($"isActive={isActive.Value}");
                    url = $"{BaseUrl}/filter?{string.Join("&", qs)}";
                }
                else
                {
                    url = BaseUrl;
                }

                _logger.LogDebugIf(Debug, "[LT_CLIENT] GetList -> {Url}", url);
                return await _http.GetAsync<List<LeaveTypeFlatViewModel>>(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LT_CLIENT] GetList error");
                return ApiResponse<List<LeaveTypeFlatViewModel>>.Fail("Không thể tải danh sách hình thức nghỉ.");
            }
        }

        // ── GetTree (client-side grouping theo IsActive) ──────────
        public async Task<ApiResponse<List<LeaveTypeGroupViewModel>>> GetTreeAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LT_CLIENT] GetTree start");

                var flatResult = await GetListAsync(ct: ct);
                if (!flatResult.IsSuccess || flatResult.Data == null)
                    return ApiResponse<List<LeaveTypeGroupViewModel>>.Fail(
                        flatResult.Message ?? "Lỗi tải dữ liệu");

                // Group: Active trước, Inactive sau
                var groups = flatResult.Data
                    .GroupBy(x => x.IsActive)
                    .OrderByDescending(g => g.Key)   // true (active) trước
                    .Select(g => new LeaveTypeGroupViewModel
                    {
                        IsActive = g.Key,
                        IsExpanded = g.Key,            // active mặc định mở
                        Items = g.OrderBy(x => x.LeaveTypeName).ToList()
                    })
                    .ToList();

                _logger.LogDebugIf(Debug,
                    "[LT_CLIENT] GetTree done: {Count} groups",
                    groups.Count);

                return ApiResponse<List<LeaveTypeGroupViewModel>>.Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LT_CLIENT] GetTree error");
                return ApiResponse<List<LeaveTypeGroupViewModel>>.Fail("Lỗi xây dựng cây dữ liệu.");
            }
        }

        // ── Create ───────────────────────────────────────────────
        public async Task<ApiResponse<object>> CreateAsync(
            LeaveTypeUpsertModel model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LT_CLIENT] Create {Code}", model.LeaveTypeCode);
                return await _http.PostAsync<object>(BaseUrl, model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LT_CLIENT] Create error");
                return ApiResponse<object>.Fail("Lỗi kết nối khi tạo hình thức nghỉ.");
            }
        }

        // ── Update ───────────────────────────────────────────────
        public async Task<ApiResponse<object>> UpdateAsync(
            LeaveTypeUpsertModel model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LT_CLIENT] Update {Id}", model.LeaveTypeId);
                return await _http.PutAsync<object>($"{BaseUrl}/{model.LeaveTypeId}", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LT_CLIENT] Update error");
                return ApiResponse<object>.Fail("Lỗi kết nối khi cập nhật.");
            }
        }

        // ── Toggle ───────────────────────────────────────────────
        public async Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LT_CLIENT] Toggle {Id}", id);
                // PATCH với body rỗng
                return await _http.PutAsync<object>($"{BaseUrl}/{id}/toggle-patch", new { }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LT_CLIENT] Toggle error");
                return ApiResponse<object>.Fail("Lỗi kết nối khi toggle.");
            }
        }

        // ── Delete ───────────────────────────────────────────────
        public async Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LT_CLIENT] Delete {Id}", id);
                return await _http.DeleteAsync<object>($"{BaseUrl}/{id}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LT_CLIENT] Delete error");
                return ApiResponse<object>.Fail("Lỗi kết nối khi xóa.");
            }
        }
    }
}
