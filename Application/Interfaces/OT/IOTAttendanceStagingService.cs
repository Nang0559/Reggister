

using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;

namespace FVN_REGISTER.Application.Interfaces.OT
{
    /// <summary>
    /// Đồng bộ dữ liệu chấm công thô từ HRM (qua usp_SyncAttendanceStaging) vào F03AttendanceStaging.
    /// Đây là pipeline giao dịch hàng ngày, tách khỏi HrmSyncJob/IHrmStagingImporter.
    /// Lưu ý: usp_SyncAttendanceStaging vẫn có runtime dependency vào
    /// usp_SyncHrmShiftMaster để bảo đảm F03 shift master được refresh trước khi resolve.
    /// </summary>
    public interface IOTAttendanceStagingService
    {
        Task<int> SyncAttendanceStagingAsync(DateTime workDate, CancellationToken ct = default);
        Task<bool> IsStagingReadyAsync(DateTime workDate, CancellationToken ct = default);
    }
}
