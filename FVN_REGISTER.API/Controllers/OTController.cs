using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Constants;
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
        private readonly IAuthorizationService _authorization;
        private readonly IOTService _otService;
        private readonly IOTQueryService _queryService;
        private readonly IApprovalWorkflowOrchestrator<OTRequestSubject> _workflow;

        public OTController(
            IOTService otService,
            IOTQueryService queryService,
            IApprovalWorkflowOrchestrator<OTRequestSubject> workflow,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<OTController> logger,
            IOptionsMonitor<AuthDebugOptions> options,
            FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService authorization)
            : base(currentUser, userLog, logger, options)
        {
            _authorization = authorization;
        {
            _otService = otService;
            _queryService = queryService;
            _workflow = workflow;
        }

        [HttpGet("combined-data")]
        public async Task<IActionResult> GetCombinedData(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var data = await _queryService.GetCombinedDataAsync(UserInfo.EmployeeCode ?? "", UserInfo.DeptCode ?? "", DateTime.Now.Year, DateTime.Now.Month, ct);
            return Ok(ApiResponse<OTCombinedDataDto>.Ok(data));
        }

        [HttpGet("dept-ot-by-date")]
        public async Task<IActionResult> GetDeptOTByDate([FromQuery] DateTime date, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<List<OTRequestDto>>.Ok(await _queryService.GetDeptByDateAsync(UserInfo.DeptCode ?? "", date, ct)));
        }

        [HttpGet("dept-employees")]
        public async Task<IActionResult> GetDeptEmployees([FromQuery] string? deptCode, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var dept = string.IsNullOrWhiteSpace(deptCode) ? UserInfo.DeptCode : deptCode;
            if (string.IsNullOrWhiteSpace(dept)) dept = await _queryService.GetEmployeeDeptCodeAsync(UserInfo.EmployeeCode ?? "", ct);
            if (string.IsNullOrWhiteSpace(dept)) return BadRequest(ApiResponse<object>.Fail("Không xác định được phòng ban."));
            return Ok(ApiResponse<List<OTEmployeeDto>>.Ok(await _queryService.GetDeptEmployeesAsync(dept, ct)));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int limit = 10, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo?.EmployeeCode == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn."));
            return Ok(ApiResponse<List<OTSummaryDto>>.Ok(await _queryService.GetRecentSummaryAsync(UserInfo.EmployeeCode, limit, ct)));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? year, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn."));
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
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<OTBalanceDto>.Ok(await _queryService.GetBalanceAsync(UserInfo.EmployeeCode ?? "", year, DateTime.Now.Month, ct)));
        }

        [HttpGet("balance/{employeeCode}/{year:int}/{month:int}")]
        public async Task<IActionResult> GetBalanceFull(string employeeCode, int year, int month, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<OTBalanceDto>.Ok(await _queryService.GetBalanceAsync(employeeCode, year, month, ct)));
        }

        [HttpGet("balance-summary/{year:int}")]
        public async Task<IActionResult> GetBalanceSummary(int year, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<OTBalanceDto>.Ok(await _queryService.GetSimpleBalanceAsync(UserInfo.EmployeeCode ?? "", year, ct)));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
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
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn."));
            ApprovalStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApprovalStatus>(status, true, out var statusValue)) parsedStatus = statusValue;
            var data = await _queryService.GetPagedAsync(deptCode, parsedStatus, fromDate, toDate, page, pageSize, ct);
            return HandleResult(ServiceResult<PaginationResult<OTSummaryDto>>.Ok(data));
        }

        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
            => HandleResult(await _queryService.GetFullDetailsAsync(id, ct));

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTApprove, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (UserInfo?.Email == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<List<PendingApprovalItemDto>>.Ok(await _workflow.GetPendingForApproverAsync(UserInfo.Email, ct)));
        }

        [HttpGet("pending-summary")]
        public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTApprove, ct)) return Forbid();
            if (UserInfo?.Email == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var items = await _workflow.GetPendingForApproverAsync(UserInfo.Email, ct);

            var summary = items
                .Select(x => new
                {
                    Item = x,
                    Level = x.ApprovalSteps
                        .Where(s => s.IsRequired && s.Decision == DecisionType.Pending)
                        .OrderBy(s => s.Level)
                        .Select(s => s.Level)
                        .FirstOrDefault()
                })
                .GroupBy(x => x.Level)
                .OrderBy(x => x.Key)
                .Select(x => new
                {
                    Level = x.Key,
                    Count = x.Count()
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(summary));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OTRequestUpsertDto dto, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTCreate, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Chưa đăng nhập."));
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Employees == null || dto.Employees.Count == 0) return BadRequest(ApiResponse<object>.Fail("Phải có ít nhất 1 nhân viên."));
            return HandleResult(await _otService.CreateAsync(dto, UserInfo, ct));
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] OTApprovalCommandDto request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTApprove, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            return HandleResult(await _otService.ApproveAsync(request.Ids, request.Level, UserInfo, request.Comment, ct));
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] OTApprovalCommandDto request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTApprove, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            if (string.IsNullOrWhiteSpace(request.Comment)) return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));
            return HandleResult(await _otService.RejectAsync(request.Ids, request.Level, UserInfo, request.Comment, ct));
        }

        [HttpPost("cancel/{id:int}")]
        public async Task<IActionResult> Cancel(int id, [FromBody] OTCancelRequestDto body, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTCancel, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.CancelAsync(id, body.Reason ?? "Hủy bởi người dùng", UserInfo, ct));
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateHours([FromBody] OTValidateHoursRequestDto body, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTView, ct)) return Forbid();
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var dto = new OTRequestUpsertDto
            {
                EmployeeCode = body.EmployeeCode,
                OTDate = body.OTDate,
                OTTypeCode = body.OTType,
                Employees = new List<OTEmployeeDto>
                {
                    new() { EmployeeCode = body.EmployeeCode, OTHours = body.Hours }
                }
            };
            return Ok(ApiResponse<OTValidationResultDto>.Ok(await _queryService.ValidateHoursAsync(dto, ct)));
        }

        [HttpPost("join/{id:int}")]
        public async Task<IActionResult> JoinOT(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTEdit, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.JoinAsync(id, UserInfo, ct));
        }

        [HttpPost("{otRequestId:int}/remove-employee/{employeeCode}")]
        public async Task<IActionResult> RemoveEmployee(int otRequestId, string employeeCode, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTEdit, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.RemoveEmployeeAsync(otRequestId, employeeCode, UserInfo, ct));
        }

        [HttpPut("{id:int}/employees/update")]
        public async Task<IActionResult> UpdateEmployeeOTInfo(int id, [FromBody] List<OTEmployeeDto> employees, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTEdit, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _otService.UpdateEmployeeOTInfoAsync(id, employees, UserInfo, ct));
        }
    }
}
