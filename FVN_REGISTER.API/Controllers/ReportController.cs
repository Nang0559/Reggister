using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Contract.Interfaces.Reports;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    // FVN_REGISTER.API/Controllers/ReportController.cs
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : BaseApiController
    {
        // Dùng IEnumerable để inject tất cả IReportService implementations
        private readonly IEnumerable<IReportService> _services;

        public ReportController(
            IEnumerable<IReportService> services,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<ReportController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<IActionResult> GetReport(
            [FromBody] ReportQueryDto query, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            // Tìm service phù hợp (Strategy pattern)
            var service = _services.FirstOrDefault(s => s.CanHandle(query.Type));
            if (service == null)
                return BadRequest(ApiResponse<object>.Fail(
                    $"Chưa hỗ trợ loại báo cáo: {query.Type}"));

            var result = await service.GetReportAsync(query, UserInfo, ct);
            await LogActionAsync($"Xem báo cáo: {query.Type}");
            return HandleResult(result);
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportExcel(
            [FromBody] ReportQueryDto query, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();

            var service = _services.FirstOrDefault(s => s.CanHandle(query.Type));
            if (service == null) return BadRequest();

            var result = await service.ExportExcelAsync(query, UserInfo, ct);
            if (!result.IsSuccess) return BadRequest(result.Message);

            await LogActionAsync($"Xuất Excel: {query.Type}");
            return File(result.Data!,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCao_{query.Type}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        [HttpGet("lookup/departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            var service = _services.FirstOrDefault(s => s.CanHandle(ReportType.LeaveBalance));
            if (service == null) return BadRequest(ApiResponse<object>.Fail("Hệ thống chưa sẵn sàng"));

            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            // Thay CurrentUser thành UserInfo 👇
            var result = await service.GetLookupDepartmentsAsync(UserInfo, ct);
            return HandleResult(result);
        }

        // =================================================================
        // BỔ SUNG 2: API TÌM KIẾM NHÂN VIÊN AUTOCOMPLETE (Dùng HTTP GET)
        // =================================================================
        [HttpGet("lookup/employees")]
        public async Task<IActionResult> SearchEmployees([FromQuery] string? filterText, [FromQuery] string? deptCode, CancellationToken ct)
        {
            var service = _services.FirstOrDefault(s => s.CanHandle(ReportType.LeaveBalance));
            if (service == null) return BadRequest(ApiResponse<object>.Fail("Hệ thống chưa sẵn sàng"));

            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            // Thay CurrentUser thành UserInfo và truyền thêm deptCode nhận từ Frontend 👇
            var result = await service.SearchLookupEmployeesAsync(filterText ?? "", UserInfo, deptCode, ct);
            return HandleResult(result);
        }
    }
}
