
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Jobs;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OTSyncController : BaseApiController
    {
        private readonly IOTAttendanceStagingService _staging;
        private readonly IOTAttendanceReconciliationService _reconciliation;
        private readonly IOTWorkerStatus _workerStatus;
        private readonly IDepartmentLookupService _departments;
        private readonly IWebHostEnvironment _environment;

        public OTSyncController(
            IOTAttendanceStagingService staging,
            IOTAttendanceReconciliationService reconciliation,
            IOTWorkerStatus workerStatus,
            IDepartmentLookupService departments,
            IWebHostEnvironment environment,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<OTSyncController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _staging = staging;
            _reconciliation = reconciliation;
            _workerStatus = workerStatus;
            _departments = departments;
            _environment = environment;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> TriggerSync(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();

            var workDate = date ?? DateTime.Today.AddDays(-1);
            var count = await _staging.SyncAttendanceStagingAsync(workDate, ct);
            return Ok(ApiResponse<object>.Ok(
                new { workDate, syncedCount = count },
                $"Đã đồng bộ {count} bản ghi chấm công vào staging."));
        }

        [HttpGet("worker-status")]
        [AllowAnonymous]
        public IActionResult GetWorkerStatus()
            => Ok(ApiResponse<OTWorkerStatusDto>.Ok(_workerStatus.GetStatus()));

        [HttpPost("worker-trigger")]
        public async Task<IActionResult> TriggerWorkerNow(
            [FromQuery] DateTime? date,
            [FromQuery] string? deptCode,
            CancellationToken ct)
        {
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
                return Forbid();

            var workDate = date ?? DateTime.Today.AddDays(-1);
            var result = await _reconciliation.ReconcileActualHoursAsync(
                workDate, deptCode, ct);
            return Ok(ApiResponse<OTReconciliationResultDto>.Ok(result, result.Summary));
        }

        [HttpPost("worker-test-run")]
        public async Task<IActionResult> TestWorkerRun(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            if (!_environment.IsDevelopment())
                return Forbid();

            var workDate = date ?? DateTime.Today.AddDays(-1);
            var syncedCount = await _staging.SyncAttendanceStagingAsync(workDate, ct);
            var result = await _reconciliation.ReconcileActualHoursAsync(
                workDate, null, ct);

            return Ok(ApiResponse<object>.Ok(new
            {
                workDate,
                syncedFromHRM = syncedCount,
                reconciliation = result
            }, $"Test OK: {syncedCount} bản ghi HRM đã staging."));
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            var departments = await _departments.GetActiveDepartmentsAsync(ct);
            return Ok(ApiResponse<object>.Ok(departments));
        }
    }
}
