using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FVN_REGISTER.Core.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/department-status")]  // ← đặt route rõ ràng, không dùng [controller]
    public class DepartmentStatusController : BaseApiController
    {
        private readonly IDepartmentStatusService _deptStatus;

        public DepartmentStatusController(
            IDepartmentStatusService deptStatus,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<DepartmentStatusController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _deptStatus = deptStatus;
        }

        // GET api/department-status/all?date=2026-06-16
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            // Chỉ Admin/SuperAdmin mới xem được toàn công ty
            if (!UserInfo!.IsAdmin() && !UserInfo.IsSuperAdmin())
                return Forbid();

            var targetDate = date ?? DateTime.Today;

            _logger.LogDebugIf(Debug,
                "[DEPT_STATUS] GetAll date={Date}",
                targetDate.ToString("dd/MM/yyyy"));

            await LogActionAsync(
                $"Xem trạng thái toàn công ty ngày {targetDate:dd/MM/yyyy}");

            var result = await _deptStatus.GetAllDeptStatusAsync(targetDate, ct);

            if (result == null || !result.Any())
                return Ok(ApiResponse<List<DepartmentStatusDto>>.Ok(
                    new List<DepartmentStatusDto>(),
                    "Không có dữ liệu."));

            _logger.LogDebugIf(Debug,
                "[DEPT_STATUS] GetAll done: {Count} phòng ban", result.Count);

            return Ok(ApiResponse<List<DepartmentStatusDto>>.Ok(result));
        }

        // GET api/department-status/{deptCode}?date=2026-06-16
        [HttpGet("{deptCode}")]
        public async Task<IActionResult> GetByDept(
            string deptCode,
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(deptCode))
                return BadRequest(ApiResponse<object>.Fail("Mã phòng ban không hợp lệ."));

            // Nhân viên thường + Approver chỉ xem được phòng của mình
            // Admin/SuperAdmin xem được tất cả
            if (!UserInfo!.IsAdmin() && !UserInfo.IsSuperAdmin())
            {
                if (!string.Equals(UserInfo.DeptCode, deptCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarnIf(Debug,
                        "[DEPT_STATUS] Forbidden: user dept={UserDept} != requested={Req}",
                        UserInfo.DeptCode, deptCode);

                    return Forbid();
                }
            }

            var targetDate = date ?? DateTime.Today;

            _logger.LogDebugIf(Debug,
                "[DEPT_STATUS] GetByDept dept={Dept} date={Date}",
                deptCode, targetDate.ToString("dd/MM/yyyy"));

            await LogActionAsync(
                $"Xem trạng thái phòng {deptCode} ngày {targetDate:dd/MM/yyyy}");

            var result = await _deptStatus.GetDeptStatusAsync(deptCode, targetDate, ct);

            if (result == null)
                return NotFound(ApiResponse<object>.Fail(
                    $"Không tìm thấy dữ liệu phòng ban {deptCode}."));

            _logger.LogDebugIf(Debug,
                "[DEPT_STATUS] GetByDept done: {Count} nhân viên | present={P} onLeave={L}",
                result.Employees?.Count ?? 0,
                result.PresentCount,
                result.OnLeaveCount);

            return Ok(ApiResponse<DepartmentStatusDto>.Ok(result));
        }
    }
}
