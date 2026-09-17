// FVN_REGISTER.API/Services/OT/OTSyncService.cs

using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTAttendanceStagingService : BaseService<OTAttendanceStagingService>, IOTAttendanceStagingService
    {
        private readonly IUnitOfWork _uow;

        public OTAttendanceStagingService(
            IUnitOfWork uow,
            ILogger<OTAttendanceStagingService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        public async Task<int> SyncAttendanceStagingAsync(DateTime workDate, CancellationToken ct = default)
        {
            try
            {
                Logger.LogInformation("[OT_STAGING] Bắt đầu sync staging ngày {Date}", workDate.Date);

                _uow.SetCommandTimeout(180);

                var dateParam = new SqlParameter("@WorkDate", SqlDbType.Date) { Value = workDate.Date };

                var result = await _uow.SqlQueryRawAsync<SyncCountResult>(
                    "EXEC dbo.usp_SyncAttendanceStaging @WorkDate",
                    ct, dateParam);

                var count = result.FirstOrDefault()?.SyncedCount ?? 0;

                Logger.LogInformation("[OT_STAGING] usp_SyncAttendanceStaging ngày {Date} → {Count} rows",
                    workDate.Date, count);

                if (count == 0)
                {
                    Logger.LogWarning(
                        "[OT_STAGING] Staging trả về 0 rows ngày {Date} — kiểm tra fn_OTActualCheckInOut hoặc dữ liệu HRM",
                        workDate.Date);
                }

                return count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_STAGING] SyncStaging lỗi ngày {Date}", workDate.Date);
                throw;
            }
            finally
            {
                _uow.SetCommandTimeout(30);
            }
        }

        public async Task<bool> IsStagingReadyAsync(DateTime workDate, CancellationToken ct = default)
        {
            try
            {
                var dateParam = new SqlParameter("@WorkDate", workDate.Date);

                var rows = await _uow.SqlQueryRawAsync<CountResult>(
                    "SELECT COUNT(1) AS Value FROM [dbo].[F03AttendanceStaging] WHERE WorkDate = @WorkDate",
                    ct, dateParam);

                var count = rows.FirstOrDefault()?.Value ?? 0;

                Logger.LogDebugIf(Debug, "[OT_STAGING] IsStagingReady ngày {Date}: {Count} rows",
                    workDate.Date, count);

                return count > 0;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_STAGING] IsStagingReady lỗi ngày {Date}", workDate.Date);
                return false;
            }
        }

        private class SyncCountResult
        {
            public int SyncedCount { get; set; }
            public DateOnly WorkDate { get; set; }
        }

        private class CountResult
        {
            public int Value { get; set; }
        }
    }
}