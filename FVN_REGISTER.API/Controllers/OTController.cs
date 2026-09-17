using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.ViewModels.Approvals;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;
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
        private readonly IApprovalWorkflowOrchestrator<OTRequestSubject> _workflow;

        public OTController(
            IOTService otService,
            IOTQueryService queryService,
            IApprovalWorkflowOrchestrator<OTRequestSubject> workflow,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<OTController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _otService = otService;
            _queryService = queryService;
            _workflow = workflow;
        }

        [HttpGet("combined-data")]
        public async Task<IActionResult> GetCombinedData(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var data = await _queryService.GetCombinedDataAsync(UserInfo.EmployeeCode ?? "", UserInfo.DeptCode ?? "", DateTime.Now.Year, DateTime.Now.Month, ct);
            return Ok(ApiResponse<OTCombinedDataDto>.Ok(data));
        }

        [HttpGet("dept-ot-by-date")]
        public async Task<IActionResult> GetDeptOTByDate([FromQuery] DateTime date, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var data = await _queryService.GetDeptByDateAsync(UserInfo.DeptCode ?? "", date, ct);
            return Ok(ApiResponse<List<OTRequestDto>>.Ok(data));
        }

        [HttpGet("dept-employees")]
        public async Task<IActionResult> GetDeptEmployees([FromQuery] string? deptCode, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var dept = string.IsNullOrWhiteSpace(deptCode) ? UserInfo.DeptCode : deptCode;
            if (string.IsNullOrWhiteSpace(dept)) dept = await _queryService.GetEmployeeDeptCodeAsync(UserInfo.EmployeeCode ?? "", ct);
            if (string.IsNullOrWhiteSpace(dept)) return BadRequest(ApiResponse<object>.Fail("Không xác định được phòng ban."));
            var employees = await _queryService.GetDeptEmployeesAsync(dept, ct);
            return Ok(ApiResponse<List<OTEmployeeDto>>.Ok(employees));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int limit = 10, CancellationToken ct = default)
        {
            if (UserInfo?.EmployeeCode == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn."));
            var data = await _queryService.GetRecentSummaryAsync(UserInfo.EmployeeCode, limit, ct);
            return Ok(ApiResponse<List<OTSummaryDto>>.Ok(data));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? year, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            ApprovalStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApprovalStatus>(status, true, out var statusValue)) parsedStatus = statusValue;
            var selectedYear = year ?? DateTime.Now.Year;
            var result = await _queryService.GetPagedAsync(UserInfo.DeptCode, parsedStatus, new DateTime(selectedYear, 1, 1), new DateTime(selectedYear, 12, 31), page, pageSize, ct);
            return HandleResult(ServiceResult<PaginationResult<OTSummaryDto>>.Ok(result));
        }

        [HttpGet("balance/{year:int}")]
        public async Task<IActionResult> GetBalance(int year, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _queryService.GetBalanceAsync(UserInfo.EmployeeCode ?? "", year, DateTime.Now.Month, ct);
            return Ok(ApiResponse<OTBalanceDto>.Ok(result));
        }

        [HttpGet("balance/{employeeCode}/{year:int}/{month:int}")]
        public async Task<IActionResult> GetBalanceFull(string employeeCode, int year, int month, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _queryService.GetBalanceAsync(employeeCode, year, month, ct);
            return Ok(ApiResponse<OTBalanceDto>.Ok(result));
        }

        [HttpGet("balance-summary/{year:int}")]
        public async Task<IActionResult> GetBalanceSummary(int year, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _queryService.GetSimpleBalanceAsync(UserInfo.EmployeeCode ?? "", year, ct);
            return Ok(ApiResponse<OTBalanceDto>.Ok(result));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var widgets = await _queryService.GetMyWidgetsAsync(UserInfo.EmployeeCode ?? "", ct);
            var balance = await _queryService.GetSimpleBalanceAsync(UserInfo.EmployeeCode ?? "", DateTime.Now.Year, ct);
            var recent = await _queryService.GetRecentSummaryAsync(UserInfo.EmployeeCode ?? "", 5, ct);
            return Ok(ApiResponse<object>.Ok(new { Widgets = widgets, MyBalance = balance, RecentRequests = recent }));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetPaged([FromQuery] string? deptCode, [FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            ApprovalStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApprovalStatus>(status, true, out var statusValue)) parsedStatus = statusValue;
            var data = await _queryService.GetPagedAsync(deptCode, parsedStatus, fromDate, toDate, page, pageSize, ct);
            return HandleResult(ServiceResult<PaginationResult<OTSummaryDto>>.Ok(data));
        }

        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
        {
            var result = await _queryService.GetFullDetailsAsync(id, ct);
            return HandleResult(result);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo?.Email == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _workflow.GetPendingForApproverAsync(UserInfo.Email, ct);
            return Ok(ApiResponse<List<PendingApprovalItemDto>>.Ok(result));
        }

        [HttpGet("pending-summary")]
        public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        {
            if (UserInfo?.Email == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var items = await _workflow.GetPendingForApproverAsync(UserInfo.Email, ct);
            var summary = items.GroupBy(x => x.CurrentApprovalLevel).OrderBy(x => x.Key).Select(x => new { Level = x.Key, Count = x.Count() }).ToList();
            return Ok(ApiResponse<object>.Ok(summary));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OTRequestUpsertDto dto, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Chưa đăng nhập."));
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Employees == null || dto.Employees.Count == 0) return BadRequest(ApiResponse<object>.Fail("Phải có ít nhất 1 nhân viên."));
            var result = await _otService.CreateAsync(dto, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveRequest request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _otService.ApproveAsync(request.Ids, request.Level, UserInfo, request.Comment, ct);
            return HandleResult(result);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] ApproveRequest request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (string.IsNullOrWhiteSpace(request.Comment)) return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));
            var result = await _otService.RejectAsync(request.Ids, request.Level, UserInfo, request.Comment, ct);
            return HandleResult(result);
        }

        [HttpPost("cancel/{id:int}")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelOTRequest body, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _otService.CancelAsync(id, body.Reason ?? "Hủy bởi người dùng", UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateHours([FromBody] ValidateOTRequest body, CancellationToken ct)
        {
            var dto = new OTRequestUpsertDto
            {
                EmployeeCode = body.EmployeeCode,
                OTDate = body.OTDate,
                OTTypeCode = body.OTType,
                Employees = new List<OTEmployeeDto> { new() { EmployeeCode = body.EmployeeCode, OTHours = body.Hours } }
            };
            var result = await _queryService.ValidateHoursAsync(dto, ct);
            return Ok(ApiResponse<OTValidationResultDto>.Ok(result));
        }

        [HttpPost("join/{id:int}")]
        public async Task<IActionResult> JoinOT(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.JoinAsync(id, UserInfo, ct));
        }

        [HttpPost("{otRequestId:int}/remove-employee/{employeeCode}")]
        public async Task<IActionResult> RemoveEmployee(int otRequestId, string employeeCode, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.RemoveEmployeeAsync(otRequestId, employeeCode, UserInfo, ct));
        }

        [HttpPut("{id:int}/employees/update")]
        public async Task<IActionResult> UpdateEmployeeOTInfo(int id, [FromBody] List<OTEmployeeDto> employees, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.UpdateEmployeeOTInfoAsync(id, employees, UserInfo, ct));
        }
    }

    public record CancelOTRequest(string? Reason);
    public record ValidateOTRequest(string EmployeeCode, DateTime OTDate, decimal Hours, string OTType);
}
