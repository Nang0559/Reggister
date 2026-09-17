// ============================================================
// OTController.cs — Fixed
// Khớp hoàn toàn với OTService hiện tại (không có ValidateAndArchiveAsync)
// OTEmployeeModel: OTHours, OTTypeCode, ActualHours, Note (không có PlannedHours)
// ============================================================

using AutoMapper;

using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Interfaces.Approvals;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models.ApprovalModels;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels.Approvals;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    /// <summary>
    /// API cho nghiệp vụ đăng ký/duyệt làm thêm giờ (OT).
    /// Toàn bộ logic duyệt (build hierarchy, xử lý quyết định, tính trạng thái tổng)
    /// nằm trong OTService + ApprovalEngine/ApprovalProvider — Controller chỉ
    /// nhận request, lấy UserInfo, gọi Service, trả kết quả.
    ///
    /// KHÔNG inject IApprovalEngine<OTRequestSubject> trực tiếp ở đây — mọi truy vấn
    /// pending đều đi qua OTService.GetMyPendingAsync/GetMyPendingSummaryAsync
    /// (dùng ApprovalListService, có enrich attachment), để chỉ có 1 cách đọc
    /// "đơn đang chờ duyệt" trong toàn hệ thống.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OTController : BaseApiController
    {
        private readonly IOTService _otService;
       

        public OTController(
            IOTService otService,
            
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<OTController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _otService = otService;
          
        }

        // ============================================================
        // QUERIES — Form / dữ liệu hỗ trợ tạo đơn
        // ============================================================

        // GET api/OT/combined-data
        [HttpGet("combined-data")]
        [HttpGet("combined-data")]
        public async Task<IActionResult> GetCombinedData(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var data = await _otService.GetCombinedDataAsync(
                UserInfo.EmployeeCode ?? "", UserInfo.DeptCode ?? "",
                DateTime.Now.Year, DateTime.Now.Month, ct);

            return Ok(ApiResponse<OTCombinedDataDto>.Ok(data));
        }

        // GET api/OT/dept-ot-by-date?date=2025-08-01
        [HttpGet("dept-ot-by-date")]
        public async Task<IActionResult> GetDeptOTByDate(
            [FromQuery] DateTime date, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var data = await _otService.GetDeptOTByDateAsync(
                UserInfo.DeptCode ?? "", date, ct);

            return Ok(ApiResponse<List<OTRequestViewModel>>.Ok(data));
        }

        // GET api/OT/dept-employees?deptCode=13
        [HttpGet("dept-employees")]
        public async Task<IActionResult> GetDeptEmployees(
         [FromQuery] string? deptCode, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var dept = deptCode;
            if (string.IsNullOrEmpty(dept))
            {
                dept = UserInfo.DeptCode;
                if (string.IsNullOrEmpty(dept))
                    dept = await _otService.GetEmployeeDeptCodeAsync(UserInfo.EmployeeCode ?? "", ct);
            }

            if (string.IsNullOrEmpty(dept))
                return BadRequest(ApiResponse<object>.Fail("Không xác định được phòng ban."));

            var employees = await _otService.GetDeptEmployeesAsync(dept, ct);
            return Ok(ApiResponse<List<OTEmployeeModel>>.Ok(employees));
        }

        // ============================================================
        // QUERIES — Lịch sử / số dư / dashboard
        // ============================================================

        // GET api/OT/recent?limit=10
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(
            [FromQuery] int limit = 10, CancellationToken ct = default)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var data = await _otService.GetRecentHistoryAsync(
                UserInfo.EmployeeCode ?? "", limit, ct);

            return Ok(ApiResponse<List<OTRequestViewModel>>.Ok(data));
        }

        // GET api/OT/history?year=2026&status=Pending&page=1&pageSize=20
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] int? year,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var filter = new HistoryFilterDto
            {
                Year = year,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var result = await _otService.GetHistoryAsync(filter, UserInfo, ct);
            return HandleResult(result);
        }

        // GET api/OT/balance/{year} — số dư giờ OT của chính user hiện tại, theo tháng hiện tại
        [HttpGet("balance/{year:int}")]
        public async Task<IActionResult> GetBalance(int year, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.GetBalanceAsync(
                UserInfo.EmployeeCode ?? "", year, DateTime.Now.Month, ct);

            return HandleResult(result);
        }

        // GET api/OT/balance/{employeeCode}/{year}/{month} — HR xem số dư NV khác
        [HttpGet("balance/{employeeCode}/{year:int}/{month:int}")]
        public async Task<IActionResult> GetBalanceFull(
            string employeeCode, int year, int month, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.GetBalanceAsync(employeeCode, year, month, ct);
            return HandleResult(result);
        }

        // GET api/OT/balance-summary/{year} — số dư tổng hợp đầy đủ qua IHistoryHandler
        // (dùng cho trang lịch sử/báo cáo, khác balance/{year} ở trên — đó là số dư
        // theo giờ/tháng cụ thể dùng cho form tạo đơn).
        [HttpGet("balance-summary/{year:int}")]
        public async Task<IActionResult> GetBalanceSummary(int year, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.GetBalanceSummaryAsync(year, UserInfo, ct);
            return HandleResult(result);
        }

        // GET api/OT/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            // 1. Lấy dữ liệu từ các service (vẫn trả về ServiceResult)
            var widgetsResult = await _otService.GetDashboardWidgetsAsync(UserInfo.Email ?? "", UserInfo.DeptCode ?? "", ct);
            var balanceResult = await _otService.GetBalanceAsync(UserInfo.EmployeeCode ?? "", DateTime.Now.Year, DateTime.Now.Month, ct);
            var recentResult = await _otService.GetRecentHistoryAsync(UserInfo.EmployeeCode ?? "", 5, ct);

            // 2. Kiểm tra lỗi (Nếu bất kỳ dịch vụ nào bắt buộc cần thiết bị lỗi, có thể trả về Fail)
            // Ở đây tôi chọn cách an toàn: nếu lỗi thì gán giá trị mặc định, nếu cần dừng lại thì dùng return
            if (!widgetsResult.IsSuccess) return Ok(ApiResponse<object>.Fail(widgetsResult.Message));

            // 3. Xây dựng ViewModel
            var vm = new OTDashboardViewModel
            {
                Widgets = widgetsResult.Data,
                // Nếu balance lỗi, gán mặc định là null hoặc một object trống
                MyBalance = balanceResult.IsSuccess ? balanceResult.Data : null,
                RecentRequests = recentResult.IsSuccess ? recentResult.Data : new List<OTHistoryDto>()
            };

            return Ok(ApiResponse<OTDashboardViewModel>.Ok(vm));
        }

        // GET api/OT/list?page=1&pageSize=20&...
        [HttpGet("list")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? deptCode,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var data = await _otService.GetPagedAsync(
                deptCode, status, fromDate, toDate, page, pageSize, ct);

            return HandlePagedResult(
                ServiceResult<PaginationResult<OTRequestViewModel>>.Ok(data));
        }

        // GET api/OT/detail/{id}
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
        {
            var result = await _otService.GetDetailsAsync(id, ct);
            return HandleResult(result);
        }

        // ============================================================
        // QUERIES — Approver (pending)
        // ============================================================

        // GET api/OT/pending — danh sách đầy đủ, gom theo Level, kèm attachment
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo?.Email == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.GetMyPendingAsync(UserInfo.Email, ct);
            return HandleResult(result);
        }

        // GET api/OT/pending-summary — chỉ đếm theo Level (dùng cho badge/widget)
        [HttpGet("pending-summary")]
        public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        {
            if (UserInfo?.Email == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.GetMyPendingSummaryAsync(UserInfo.Email, ct);
            return HandleResult(result);
        }

        // ============================================================
        // COMMANDS — Tạo / duyệt / từ chối / hủy
        // ============================================================

        // POST api/OT/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromBody] OTRequestUpsertDto dto, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Chưa đăng nhập."));

            if (dto == null)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            if (dto.Employees == null || !dto.Employees.Any())
                return BadRequest(ApiResponse<object>.Fail("Phải có ít nhất 1 nhân viên."));

            var model = new CreateOTRequestModel
            {
                EmployeeCode = dto.EmployeeCode,
                DeptCode = dto.DeptCode,
                CvCode = dto.CvCode,
                WorkYear = dto.OTDate.Year,
                OTDate = dto.OTDate,
                OTTypeCode = dto.OTTypeCode,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ScopeType = dto.ScopeType,
                AttachedDocuments = dto.AttachedDocuments,
                Employees = dto.Employees.Select(e => new OTEmployeeModel
                {
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    DeptCode = e.DeptCode,
                    DeptName = e.DeptName,
                    CvCode = e.CvCode,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    OTHours = e.OTHours,
                    OTReasonCategoryCode = e.OTReasonCategoryCode ?? OTReasonCategory.Other,
                    OTReasonDetail = e.OTReasonDetail,
                    Note = e.Note
                }).ToList()
            };

            await LogActionAsync(
                $"Tạo đơn OT ngày {dto.OTDate:dd/MM/yyyy} - {dto.Employees.Count} nhân viên");

            var result = await _otService.CreateAsync(model, UserInfo, ct);
            return HandleResult(result);
        }

        // POST api/OT/approve
        [HttpPost("approve")]
        public async Task<IActionResult> Approve(
            [FromBody] ApproveRequest request, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync(
                $"Duyệt OT Level {request.Level}: {string.Join(",", request.Ids)}");

            var result = await _otService.ApproveAsync(
                request.Ids, request.Level, UserInfo, request.Comment, ct);

            return HandleResult(result);
        }

        // POST api/OT/reject
        [HttpPost("reject")]
        public async Task<IActionResult> Reject(
            [FromBody] ApproveRequest request, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            // Validate input-contract ở Controller — OTService.RejectAsync cũng tự check lại
            // (defense-in-depth), nhưng chặn sớm ở đây tránh round-trip không cần thiết.
            if (string.IsNullOrWhiteSpace(request.Comment))
                return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));

            await LogActionAsync(
                $"Từ chối OT Level {request.Level}: {string.Join(",", request.Ids)}");

            var result = await _otService.RejectAsync(
                request.Ids, request.Level, UserInfo, request.Comment!, ct);

            return HandleResult(result);
        }

        // POST api/OT/cancel/{id}
        // CHỈ giữ 1 route cancel/{id} duy nhất — bản gốc có 2 method trùng route
        // (Cancel + CancelOT), ASP.NET sẽ throw AmbiguousMatchException lúc runtime.
        [HttpPost("cancel/{id:int}")]
        public async Task<IActionResult> Cancel(
            int id, [FromBody] CancelOTRequest body, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync($"Hủy đơn OT ID: {id}");

            var result = await _otService.CancelAsync(
                id, body.Reason ?? "Hủy bởi người dùng", UserInfo, ct);

            return HandleResult(result);
        }

        // POST api/OT/validate
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateHours(
            [FromBody] ValidateOTRequest body, CancellationToken ct)
        {
            var model = new CreateOTRequestModel
            {
                EmployeeCode = body.EmployeeCode,
                OTDate = body.OTDate,
                OTTypeCode = body.OTType,
                Employees = new List<OTEmployeeModel>
                {
                    new()
                    {
                        EmployeeCode = body.EmployeeCode,
                        OTHours = body.Hours,
                        OTTypeCode = body.OTType
                    }
                }
            };

            var result = await _otService.ValidateHoursAsync(model, ct);
            return HandleResult(result);
        }

        // ============================================================
        // COMMANDS — Thành viên trong đơn OT
        // ============================================================

        // POST api/OT/join/{id}
        [HttpPost("join/{id:int}")]
        public async Task<IActionResult> JoinOT(int id, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.JoinAsync(id, UserInfo, ct);
            return HandleResult(result);
        }

        // POST api/OT/{otRequestId}/remove-employee/{employeeCode}
        [HttpPost("{otRequestId:int}/remove-employee/{employeeCode}")]
        public async Task<IActionResult> RemoveEmployee(
            int otRequestId, string employeeCode, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync($"Xóa NV {employeeCode} khỏi đơn OT {otRequestId}");

            var result = await _otService.RemoveEmployeeAsync(
                otRequestId, employeeCode, UserInfo, ct);

            return HandleResult(result);
        }

        // PUT api/OT/{id}/employees/update
        [HttpPut("{id:int}/employees/update")]
        public async Task<IActionResult> UpdateEmployeeOTInfo(
            int id, [FromBody] List<OTEmployeeModel> employees, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.UpdateEmployeeOTInfoAsync(id, employees, UserInfo, ct);

            await LogActionAsync($"Cập nhật giờ OT đơn #{id}");

            return HandleResult(result);
        }
    }

    // ----------------------------------------------------------------
    // Request body DTOs — nội bộ controller, không phải ViewModels
    // ----------------------------------------------------------------

    /// <summary>Body cho POST /cancel/{id}</summary>
    public record CancelOTRequest(string? Reason);

    /// <summary>Body cho POST /validate</summary>
    public record ValidateOTRequest(
        string EmployeeCode,
        DateTime OTDate,
        decimal Hours,
        string OTType);
}