using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public class OTClientService : IOTClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<OTClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;
        private const string BASE = "api/OT";

        public OTClientService(
            IHttpClientWithAuth http,
            ILogger<OTClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        // ============================================================
        // COMMANDS
        // ============================================================

        public async Task<ApiResponse<object>> CreateOTRequestAsync(
    CreateOTRequestModel model,
    CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Create date={Date} employees={Count}",
                    model.OTDate, model.Employees.Count);

                // 🔥 Map sang DTO gọn trước khi gửi
                var dto = new CreateOTRequestDto
                {
                    EmployeeCode = model.EmployeeCode,
                    DeptCode = model.DeptCode,
                    CvCode = model.CvCode,
                    OTDate = model.OTDate,
                    OTTypeCode = model.OTTypeCode,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    ScopeType = model.ScopeType,
                    AttachedDocuments = model.AttachedDocuments,

                    Level3ApproveEmail = model.Level3ApproveEmail,
                    Level3ApproveCode = model.Level3ApproveCode,
                    Level3ApproveName = model.Level3ApproveName,
                    Level5ApproveEmail = model.Level5ApproveEmail,
                    Level5ApproveCode = model.Level5ApproveCode,
                    Level5ApproveName = model.Level5ApproveName,
                    Level6ApproveEmail = model.Level6ApproveEmail,
                    Level6ApproveCode = model.Level6ApproveCode,
                    Level6ApproveName = model.Level6ApproveName,
                    Level7ApproveEmail = model.Level7ApproveEmail,
                    Level7ApproveCode = model.Level7ApproveCode,
                    Level7ApproveName = model.Level7ApproveName,

                    Employees = model.Employees.Select(e => new OTEmployeeDto
                    {
                        EmployeeCode = e.EmployeeCode,
                        EmployeeName = e.EmployeeName,
                        DeptCode = e.DeptCode,
                        DeptName = e.DeptName,
                        CvCode = e.CvCode,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        OTHours = e.OTHours,
                        OTReasonCategoryCode = e.OTReasonCategoryCode,
                        OTReasonDetail = e.OTReasonDetail,
                        Note = e.Note
                    }).ToList()
                };

                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] DTO employees={Count} HasWorker={W}",
                    dto.Employees.Count,
                    dto.Employees.Any(e => e.CvCode == "0003"));

                return await _http.PostAsync<object>($"{BASE}/create", dto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Create ERROR");
                return ApiResponse<object>.Fail("Không thể tạo đơn OT.");
            }
        }

        public async Task<ApiResponse<object>> ApproveAsync(
            OTApproveRequest request,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Approve level={Level}", request.Level);
                return await _http.PostAsync<object>($"{BASE}/approve", request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Approve ERROR");
                return ApiResponse<object>.Fail("Không thể phê duyệt.");
            }
        }

        public async Task<ApiResponse<object>> RejectAsync(
            OTApproveRequest request,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Reject level={Level}", request.Level);
                return await _http.PostAsync<object>($"{BASE}/reject", request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Reject ERROR");
                return ApiResponse<object>.Fail("Không thể từ chối.");
            }
        }

        public async Task<ApiResponse<object>> CancelAsync(
            int otRequestId,
            string? reason,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] Cancel OT {Id}", otRequestId);
                // Controller nhận record CancelOTRequest(string? Reason)
                return await _http.PostAsync<object>(
                    $"{BASE}/cancel/{otRequestId}",
                    new { Reason = reason },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Cancel ERROR");
                return ApiResponse<object>.Fail("Không thể hủy đơn OT.");
            }
        }

        // ============================================================
        // QUERIES
        // ============================================================
        public async Task<ApiResponse<List<OTRequestViewModel>>> GetDeptOTByDateAsync(
    DateTime date, CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<List<OTRequestViewModel>>(
                    $"{BASE}/dept-ot-by-date?date={date:yyyy-MM-dd}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetDeptOTByDate ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được đơn OT trong ngày.");
            }
        }
        public async Task<ApiResponse<OTCombinedDataDto>> GetCombinedDataAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetCombinedData");
                return await _http.GetAsync<OTCombinedDataDto>(
                    $"{BASE}/combined-data", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetCombinedData ERROR");
                return ApiResponse<OTCombinedDataDto>.Fail("Không tải được dữ liệu trang OT.");
            }
        }
        public Task<ApiResponse<List<OTEmployeeModel>>> GetDeptEmployeesAsync(
        string? deptCode = null, CancellationToken ct = default)
        {
            var url = string.IsNullOrWhiteSpace(deptCode)
                ? "api/OT/dept-employees"
                : $"api/OT/dept-employees?deptCode={deptCode}";
            return _http.GetAsync<List<OTEmployeeModel>>(url, ct);
        }
        public Task<ApiResponse<List<OTRequestViewModel>>> GetMyHistoryAsync(
        int? year = null, CancellationToken ct = default)
        {
            var url = year.HasValue
                ? $"api/OT/history?year={year}"
                : "api/OT/history";
            return _http.GetAsync<List<OTRequestViewModel>>(url, ct);
        }
        public async Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(
            int year,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetBalance year={Year}", year);
                // Controller: GET api/OT/balance/{year}
                return await _http.GetAsync<OTBalanceDto>(
                    $"{BASE}/balance/{year}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetBalance ERROR");
                return ApiResponse<OTBalanceDto>.Fail("Không tải được số dư OT.");
            }
        }

        public async Task<ApiResponse<OTRequestViewModel>> GetOTDetailAsync(
            int otRequestId,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetDetail id={Id}", otRequestId);
                // Controller: GET api/OT/detail/{id}
                return await _http.GetAsync<OTRequestViewModel>(
                    $"{BASE}/detail/{otRequestId}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetDetail ERROR");
                return ApiResponse<OTRequestViewModel>.Fail("Không tải được chi tiết đơn OT.");
            }
        }

        public async Task<ApiResponse<List<OTRequestViewModel>>> GetRecentOTRequestsAsync(
            int limit = 10,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetRecent limit={L}", limit);
                return await _http.GetAsync<List<OTRequestViewModel>>(
                    $"{BASE}/recent?limit={limit}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetRecent ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được lịch sử OT.");
            }
        }

        public async Task<ApiResponse<List<OTRequestViewModel>>> GetPendingApprovalsAsync(
            int level = 0,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetPending level={L}", level);
                return await _http.GetAsync<List<OTRequestViewModel>>(
                    $"{BASE}/pending?level={level}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetPending ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được danh sách chờ duyệt.");
            }
        }

        public async Task<ApiResponse<PaginationResult<OTRequestViewModel>>> GetPagedOTRequestsAsync(
            string? deptCode = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                var url = $"{BASE}/list?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(deptCode)) url += $"&deptCode={deptCode}";
                if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
                if (fromDate.HasValue) url += $"&fromDate={fromDate:yyyy-MM-dd}";
                if (toDate.HasValue) url += $"&toDate={toDate:yyyy-MM-dd}";

                return await _http.GetPagedAsync<OTRequestViewModel>(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetPaged ERROR");
                return ApiResponse<PaginationResult<OTRequestViewModel>>.Fail(
                    "Không tải được danh sách đơn OT.");
            }
        }

        public async Task<ApiResponse<OTDashboardViewModel>> GetOTDashboardAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetDashboard");
                return await _http.GetAsync<OTDashboardViewModel>(
                    $"{BASE}/dashboard", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetDashboard ERROR");
                return ApiResponse<OTDashboardViewModel>.Fail("Không tải được dashboard OT.");
            }
        }
        // THÊM method mới vào OTClientService:
        // Thêm vào OTClientService:
        public async Task<ApiResponse<OTBalanceDto>> GetEmployeeBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] GetEmployeeBalance emp={E} year={Y}", employeeCode, year);
                return await _http.GetAsync<OTBalanceDto>(
                    $"{BASE}/balance/employee/{employeeCode}/{year}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetEmployeeBalance ERROR");
                return ApiResponse<OTBalanceDto>.Fail("Không tải được số dư OT nhân viên.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="otDate"></param>
        /// <param name="hours"></param>
        /// <param name="otType"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiResponse<OTRequestViewModel>> GetDetailAsync(
    int id, CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<OTRequestViewModel>($"{BASE}/detail/{id}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetDetail ERROR Id={Id}", id);
                return ApiResponse<OTRequestViewModel>.Fail("Lỗi tải chi tiết đơn OT.");
            }
        }

        public async Task<ApiResponse<object>> JoinOTRequestAsync(
            int id, CancellationToken ct = default)
        {
            try
            {
                return await _http.PostAsync<object>($"{BASE}/join/{id}", new { }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] JoinOT ERROR Id={Id}", id);
                return ApiResponse<object>.Fail("Lỗi tham gia đơn OT.");
            }
        }
        public async Task<ApiResponse<object>> RemoveEmployeeAsync(
    int otRequestId,
    string employeeCode,
    CancellationToken ct = default)
        {
            try
            {
                return await _http.PostAsync<object>(
                    $"{BASE}/{otRequestId}/remove-employee/{employeeCode}",
                    new { },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] RemoveEmployee ERROR");
                return ApiResponse<object>.Fail("Lỗi xóa thành viên.");
            }
        }
        public async Task<ApiResponse<object>> CancelOTRequestAsync(
            int id, string reason, CancellationToken ct = default)
        {
            try
            {
                return await _http.PostAsync<object>(
                    $"{BASE}/cancel/{id}",
                    new { Reason = reason },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] CancelOT ERROR Id={Id}", id);
                return ApiResponse<object>.Fail("Lỗi hủy đơn OT.");
            }
        }
        // FVN_REGISTER.Shared/Services/OTs/OTClientService.cs

        public async Task<ApiResponse<object>> UpdateEmployeeOTInfoAsync(
            int otRequestId,
            List<OTEmployeeModel> employees,
            CancellationToken ct = default)
        {
            try
            {
                var url = $"api/OT/{otRequestId}/employees/update";
                return await _http.PutAsync<object>(url, employees, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] UpdateEmployeeOTInfo lỗi");
                return ApiResponse<object>.Fail("Không thể kết nối server.");
            }
        }
        public async Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(
            string employeeCode,
            DateTime otDate,
            decimal hours,
            string otType,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Validate emp={E} hours={H}", employeeCode, hours);
                // Controller nhận record ValidateOTRequest(EmployeeCode, OTDate, Hours, OTType)
                return await _http.PostAsync<OTValidationResultDto>($"{BASE}/validate", new
                {
                    EmployeeCode = employeeCode,
                    OTDate = otDate,
                    Hours = hours,
                    OTType = otType
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Validate ERROR");
                return ApiResponse<OTValidationResultDto>.Fail("Không thể kiểm tra giờ OT.");
            }
        }
        
    }
}

