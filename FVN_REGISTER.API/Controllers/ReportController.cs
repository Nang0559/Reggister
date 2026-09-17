using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Enums;
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
        private readonly IEnumerable<IReportService> _services;

        public ReportController(IEnumerable<IReportService> services, ICurrentUserService currentUser, IUserLogService userLog, IMapper mapper, ILogger<ReportController> logger, IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<IActionResult> GetReport([FromBody] ReportQueryDto query, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            var service = _services.FirstOrDefault(s => s.CanHandle(query.Type));
            if (service == null) return BadRequest(ApiResponse<object>.Fail($"Chưa hỗ trợ loại báo cáo: {query.Type}"));
            return HandleResult(await service.GetReportAsync(query, UserInfo, ct));
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportExcel([FromBody] ReportQueryDto query, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var service = _services.FirstOrDefault(s => s.CanHandle(query.Type));
            if (service == null) return BadRequest();
            var result = await service.ExportExcelAsync(query, UserInfo, ct);
            if (!result.IsSuccess || result.Data == null) return BadRequest(result.Message);
            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_{query.Type}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet("lookup/departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            var service = _services.FirstOrDefault(s => s.CanHandle(ReportType.LeaveBalance));
            if (service == null) return BadRequest(ApiResponse<object>.Fail("Hệ thống chưa sẵn sàng"));
            return HandleResult(await service.GetLookupDepartmentsAsync(UserInfo, ct));
        }

        [HttpGet("lookup/employees")]
        public async Task<IActionResult> SearchEmployees([FromQuery] string? filterText, [FromQuery] string? deptCode, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            var service = _services.FirstOrDefault(s => s.CanHandle(ReportType.LeaveBalance));
            if (service == null) return BadRequest(ApiResponse<object>.Fail("Hệ thống chưa sẵn sàng"));
            return HandleResult(await service.SearchLookupEmployeesAsync(filterText ?? "", UserInfo, deptCode, ct));
        }
    }
}
