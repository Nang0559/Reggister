using AutoMapper;
using FVN_REGISTER.API.Services.OT;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;

using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OTSyncController : BaseApiController
    {
        private readonly IOTSyncService _syncService;
        private readonly IOTWorkerStatus _workerStatus; // 🔥 inject interface

        public OTSyncController(
            IOTSyncService syncService,
            IOTWorkerStatus workerStatus,           // 🔥 thêm
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<OTSyncController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _syncService = syncService;
            _workerStatus = workerStatus;           // 🔥 gán
        }

        // ── GET unconfirmed ───────────────────────────────────
        [HttpGet("unconfirmed")]
        public async Task<IActionResult> GetUnconfirmed(
            [FromQuery] DateTime? date,
            [FromQuery] string? deptCode,
            CancellationToken ct)
        {
            var workDate = date ?? DateTime.Today.AddDays(-1);

            var list = await _syncService.GetUnconfirmedOTAsync(workDate, deptCode, ct);

            return HandleResult(ServiceResult<OTUnconfirmedListResponse>.Ok(
                new OTUnconfirmedListResponse
                {
                    Date = workDate,
                    Count = list.Count,
                    Items = list
                }));
        }

        // ── POST sync thủ công ────────────────────────────────
        [HttpPost("sync")]
        public async Task<IActionResult> TriggerSync(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var workDate = date ?? DateTime.Today.AddDays(-1);

            await LogActionAsync($"Trigger OT sync ngày {workDate:dd/MM/yyyy}");

            // Fire-and-forget với scope riêng — không bị cancel khi HTTP disconnect
            _ = Task.Run(async () =>
            {
                using var scope = HttpContext.RequestServices
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IOTSyncService>();

                try
                {
                    await svc.SyncAttendanceStagingAsync(workDate, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    scope.ServiceProvider
                        .GetRequiredService<ILogger<OTSyncController>>()
                        .LogError(ex, "[OT_SYNC] Background sync lỗi ngày {Date}", workDate);
                }
            });

            return Ok(ApiResponse<object>.Ok(
                new { workDate = workDate.ToString("dd/MM/yyyy") },
                $"Đã bắt đầu sync ngày {workDate:dd/MM/yyyy}. Khoảng 45-60s sẽ xong."));
        }

        // ── GET worker-status — ĐỌC TỪ WORKER THỰC TẾ ───────
        [HttpGet("worker-status")]
        [AllowAnonymous]
        public IActionResult GetWorkerStatus()
        {
            // 🔥 Dùng _workerStatus đã inject — không hardcode "RUNNING" nữa
            var status = _workerStatus.GetStatus();
            return Ok(ApiResponse<OTWorkerStatusDto>.Ok(status));
        }

        // ── POST worker-trigger (Admin) ───────────────────────
        [HttpPost("worker-trigger")]
        public async Task<IActionResult> TriggerWorkerNow(
            [FromQuery] string? deptCode,
            CancellationToken ct)
        {
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
                return Forbid();

            var workDate = DateTime.Today.AddDays(-1);

            await LogActionAsync($"Manual trigger OT worker ngày {workDate:dd/MM/yyyy}");

            await using var scope = HttpContext.RequestServices.CreateAsyncScope();
            var syncSvc = scope.ServiceProvider.GetRequiredService<IOTSyncService>();

            var result = await syncSvc.SyncActualHoursAsync(workDate, deptCode, ct);

            return HandleResult(ServiceResult<OTSyncResultDto>.Ok(
                result, result.Message));
        }

        // ── POST worker-test-run (Development only) ───────────
        [HttpPost("worker-test-run")]
        public async Task<IActionResult> TestWorkerRun(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            if (!HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment())
                return Forbid();

            var workDate = date ?? DateTime.Today.AddDays(-1);

            await using var scope = HttpContext.RequestServices.CreateAsyncScope();
            var syncSvc = scope.ServiceProvider.GetRequiredService<IOTSyncService>();

            var syncedCount = await syncSvc.SyncAttendanceStagingAsync(
                workDate, CancellationToken.None);

            _logger.LogInformation(
                "[OT_SYNC] TestWorkerRun: Synced {Count} rows vào staging", syncedCount);

            var result = await syncSvc.SyncActualHoursAsync(
                workDate, null, CancellationToken.None);

            result.SyncedFromHRM = syncedCount;

            return HandleResult(ServiceResult<OTSyncResultDto>.Ok(result,
                $"Test OK: {syncedCount} bản ghi từ HRM, {result.SyncedCount} đơn cập nhật."));
        }

        // ── GET departments ───────────────────────────────────
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            try
            {
                var depts = await _syncService.GetDepartmentsAsync(ct);
                return Ok(ApiResponse<List<DeptOption>>.Ok(depts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_SYNC] GetDepartments error");
                return BadRequest(ApiResponse<List<DeptOption>>.Fail("Lỗi tải phòng ban"));
            }
        }
    }
}
