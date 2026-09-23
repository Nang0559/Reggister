
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : BaseApiController
    {
        private readonly IReportDispatcher _reportDispatcher;
        private readonly IAuthorizationService _authorization;

        public ReportController(
            IReportDispatcher reportDispatcher,
            IAuthorizationService authorization,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            
            ILogger<ReportController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _reportDispatcher = reportDispatcher;
            _authorization = authorization;
        }

        [HttpPost]
        public async Task<IActionResult> GetReport(
            [FromBody] ReportQueryDto query,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var scopeOk = await CanAccessRequestedScopeAsync(query, ct);
            if (!scopeOk)
                return Forbid();

            return HandleResult(await _reportDispatcher.GetReportAsync(query, UserInfo, ct));
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportExcel(
            [FromBody] ReportQueryDto query,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            if (!await CanAccessRequestedScopeAsync(query, ct))
                return Forbid();

            var result = await _reportDispatcher.ExportExcelAsync(query, UserInfo, ct);
            if (!result.IsSuccess || result.Data == null)
                return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Không thể xuất báo cáo."));

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCao_{query.Type}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        private async Task<bool> CanAccessRequestedScopeAsync(
            ReportQueryDto query,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return false;

            if (!string.IsNullOrWhiteSpace(query.EmployeeCode))
            {
                var targetEmployee = await _authorization.CanAccessAsync(
                    UserInfo,
                    SecurityFunctionCodes.LeaveView,
                    query.EmployeeCode,
                    null,
                    ct);
                return targetEmployee;
            }

            if (!string.IsNullOrWhiteSpace(query.DeptCode))
            {
                return await _authorization.CanAccessAsync(
                    UserInfo,
                    SecurityFunctionCodes.LeaveView,
                    null,
                    query.DeptCode,
                    ct)
                    || await _authorization.CanAccessAsync(
                        UserInfo,
                        SecurityFunctionCodes.OTView,
                        null,
                        query.DeptCode,
                        ct)
                    || await _authorization.CanAccessAsync(
                        UserInfo,
                        SecurityFunctionCodes.TripView,
                        null,
                        query.DeptCode,
                        ct)
                    || await _authorization.CanAccessAsync(
                        UserInfo,
                        SecurityFunctionCodes.EquipmentView,
                        null,
                        query.DeptCode,
                        ct)
                    || await _authorization.CanAccessAsync(
                        UserInfo,
                        SecurityFunctionCodes.AttendanceView,
                        null,
                        query.DeptCode,
                        ct);
            }

            // Domain report services still enforce their own required capability and
            // reject report types that need an explicit scope. ManagedScope is applied
            // when a target employee/department is requested.
            return true;
        }

        [HttpGet("lookup/departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            return HandleResult(await _reportDispatcher.GetLookupDepartmentsAsync(UserInfo, ct));
        }

        [HttpGet("lookup/employees")]
        public async Task<IActionResult> SearchEmployees(
            [FromQuery] string? filterText,
            [FromQuery] string? deptCode,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            return HandleResult(await _reportDispatcher.SearchLookupEmployeesAsync(
                filterText ?? string.Empty,
                UserInfo,
                deptCode,
                ct));
        }
    }
}
