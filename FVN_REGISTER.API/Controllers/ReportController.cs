using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Configurations;
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

        public ReportController(
            IReportDispatcher reportDispatcher,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<ReportController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _reportDispatcher = reportDispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> GetReport(
            [FromBody] ReportQueryDto query,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            return HandleResult(await _reportDispatcher.GetReportAsync(query, UserInfo, ct));
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportExcel(
            [FromBody] ReportQueryDto query,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _reportDispatcher.ExportExcelAsync(query, UserInfo, ct);
            if (!result.IsSuccess || result.Data == null)
                return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Không thể xuất báo cáo."));

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCao_{query.Type}_{DateTime.Now:yyyyMMdd}.xlsx");
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
