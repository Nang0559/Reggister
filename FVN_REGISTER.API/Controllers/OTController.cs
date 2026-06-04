// ============================================================
// OTController.cs — Fixed
// Khớp hoàn toàn với OTService hiện tại (không có ValidateAndArchiveAsync)
// OTEmployeeModel: OTHours, OTTypeCode, ActualHours, Note (không có PlannedHours)
// ============================================================

using AutoMapper;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OTController : BaseApiController
    {
        private readonly IOTService _otService;
        private readonly IOTQueryService _queryService;

        public OTController(
            IOTService otService,
            IOTQueryService queryService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<OTController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _otService = otService;
            _queryService = queryService;
        }

        // ============================================================
        // QUERIES
        // ============================================================

        // GET api/OT/combined-data
        [HttpGet("combined-data")]
        public async Task<IActionResult> GetCombinedData(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var data = await _queryService.GetCombinedDataAsync(
                UserInfo.EmployeeCode ?? "",
                UserInfo.DeptCode ?? "",
                UserInfo.CvCode ?? "",
                DateTime.Now.Year,
                ct);

            return Ok(ApiResponse<CombinedOTViewModel>.Ok(data));
        }

        // GET api/OT/detail/{id}
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
        {
            var result = await _otService.GetDetailsAsync(id, ct);
            return HandleResult(result);
        }

        // GET api/OT/balance/{year}  — user hiện tại
        [HttpGet("balance/{year:int}")]
        public async Task<IActionResult> GetBalance(int year, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _otService.GetBalanceAsync(
                UserInfo.EmployeeCode ?? "",
                year,
                DateTime.Now.Month,
                ct);

            return HandleResult(result);
        }

        // GET api/OT/balance/{employeeCode}/{year}/{month}  — HR xem NV khác
        [HttpGet("balance/{employeeCode}/{year:int}/{month:int}")]
        public async Task<IActionResult> GetBalanceFull(
            string employeeCode, int year, int month, CancellationToken ct)
        {
            var result = await _otService.GetBalanceAsync(employeeCode, year, month, ct);
            return HandleResult(result);
        }

        // GET api/OT/recent?limit=10
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var data = await _queryService.GetRecentHistoryAsync(
                UserInfo.EmployeeCode ?? "", limit, ct);

            return Ok(ApiResponse<List<OTRequestViewModel>>.Ok(data));
        }

        // GET api/OT/pending?level=0
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(
            [FromQuery] int level = 0,
            CancellationToken ct = default)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var groups = await _queryService.GetPendingDetailsAsync(
                UserInfo.Email ?? "", ct);

            if (level > 0)
                groups = groups.Where(g => g.ApproverLevel == level).ToList();

            var allRequests = groups.SelectMany(g => g.Requests).ToList();
            return Ok(ApiResponse<List<OTRequestViewModel>>.Ok(allRequests));
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

            var data = await _queryService.GetPagedAsync(
                deptCode, status, fromDate, toDate, page, pageSize, ct);

            return HandlePagedResult(
                ServiceResult<PaginationResult<OTRequestViewModel>>.Ok(data));
        }

        // GET api/OT/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var widgets = await _queryService.GetDashboardWidgetsAsync(
                UserInfo.Email ?? "",
                UserInfo.DeptCode ?? "",
                ct);

            var balance = await _queryService.GetBalanceAsync(
                UserInfo.EmployeeCode ?? "",
                DateTime.Now.Year,
                DateTime.Now.Month,
                ct);

            var recent = await _queryService.GetRecentHistoryAsync(
                UserInfo.EmployeeCode ?? "", 5, ct);

            var vm = new OTDashboardViewModel
            {
                Widgets = widgets,
                Balance = balance,
                RecentRequests = recent
            };

            return Ok(ApiResponse<OTDashboardViewModel>.Ok(vm));
        }

        // ============================================================
        // COMMANDS
        // ============================================================

        // POST api/OT  hoặc  POST api/OT/create
        // OTService.CreateAsync → ServiceResult<int> (Id đơn mới)
        [HttpPost]
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOTRequestModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync($"Tạo đơn OT ngày {model.OTDate:dd/MM/yyyy}");

            // CreateAsync(model, user, ct) → ServiceResult<int>
            var result = await _otService.CreateAsync(model, UserInfo, ct);
            return HandleResult(result);
        }

        // POST api/OT/approve
        // OTService.ApproveAsync(ids, level, user, comment?, ct)
        [HttpPost("approve")]
        public async Task<IActionResult> Approve(
            [FromBody] OTApproveRequest request,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync(
                $"Duyệt OT Level {request.Level}: {string.Join(",", request.Ids)}");

            var result = await _otService.ApproveAsync(
                request.Ids,
                request.Level,
                UserInfo,
                request.Comment,
                ct);

            return HandleResult(result);
        }

        // POST api/OT/reject
        // OTService.RejectAsync(ids, level, user, comment, ct)
        // Lưu ý: comment là string (không nullable) trong OTService
        [HttpPost("reject")]
        public async Task<IActionResult> Reject(
            [FromBody] OTApproveRequest request,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            if (string.IsNullOrWhiteSpace(request.Comment))
                return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));

            await LogActionAsync(
                $"Từ chối OT Level {request.Level}: {string.Join(",", request.Ids)}");

            var result = await _otService.RejectAsync(
                request.Ids,
                request.Level,
                UserInfo,
                request.Comment!,   // OTService nhận string (non-nullable)
                ct);

            return HandleResult(result);
        }

        // POST api/OT/cancel/{id}
        // OTService.CancelAsync(id, reason, user, ct)
        // reason là string (non-nullable) trong OTService
        [HttpPost("cancel/{id:int}")]
        public async Task<IActionResult> Cancel(
            int id,
            [FromBody] CancelOTRequest body,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync($"Hủy đơn OT ID: {id}");

            var result = await _otService.CancelAsync(
                id,
                body.Reason ?? "",  // OTService nhận string non-nullable
                UserInfo,
                ct);

            return HandleResult(result);
        }

        // POST api/OT/validate
        // OTService.ValidateHoursAsync(model, ct) → ServiceResult<OTValidationResultDto>
        // Client gửi: { EmployeeCode, OTDate, Hours, OTType }
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateHours(
            [FromBody] ValidateOTRequest body,
            CancellationToken ct)
        {
            // Build minimal CreateOTRequestModel để truyền vào service
            var model = new CreateOTRequestModel
            {
                EmployeeCode = body.EmployeeCode,
                OTDate = body.OTDate,
                OTTypeCode = body.OTType,
                // OTEmployeeModel dùng OTHours (không phải PlannedHours)
                Employees = new List<OTEmployeeModel>
                {
                    new()
                    {
                        EmployeeCode = body.EmployeeCode,
                        OTHours      = body.Hours,         // ← đúng field của OTEmployeeModel
                        OTTypeCode   = body.OTType
                    }
                }
            };

            var result = await _otService.ValidateHoursAsync(model, ct);
            return HandleResult(result);
        }

        // ── KHÔNG có endpoint /archive ──
        // ValidateAndArchiveAsync không tồn tại trong IOTService hiện tại.
        // Nếu cần thêm sau: implement trong OTService trước, rồi thêm endpoint này.
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

    /// <summary>ViewModel cho GET /dashboard</summary>
    public class OTDashboardViewModel
    {
        public List<WidgetCounterDto> Widgets { get; set; } = new();
        public OTBalanceDto? Balance { get; set; }
        public List<OTRequestViewModel> RecentRequests { get; set; } = new();
    }
}