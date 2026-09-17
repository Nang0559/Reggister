

using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;

namespace FVN_REGISTER.Application.Interfaces.OT
{
    /// <summary>
    /// Đồng bộ dữ liệu chấm công thô từ HRM (qua usp_SyncAttendanceStaging) vào F03AttendanceStaging.
    /// Độc lập hoàn toàn với pipeline HrmSyncJob/IHrmStagingImporter (đó là master data,
    /// đây là dữ liệu giao dịch hàng ngày — 2 cơ chế khác nhau).
    /// </summary>
    public interface IOTAttendanceStagingService
    {
        Task<int> SyncAttendanceStagingAsync(DateTime workDate, CancellationToken ct = default);
        Task<bool> IsStagingReadyAsync(DateTime workDate, CancellationToken ct = default);
    }
}
