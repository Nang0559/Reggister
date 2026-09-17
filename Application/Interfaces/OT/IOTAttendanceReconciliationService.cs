using FVN_REGISTER.Contract.Dtos.OT;


namespace FVN_REGISTER.Application.Interfaces.OT
{
    /// <summary>
    /// Đối chiếu giờ OT thực tế với đơn đã đăng ký, ghi ActualHours vào F03OTEmployee,
    /// trả báo cáo kết quả.
    ///
    /// ⚠️ ĐÂY LÀ COMMAND, KHÔNG PHẢI QUERY — dù chỉ gọi để "xem" báo cáo, method này vẫn
    /// UPDATE F03OTEmployee mỗi lần chạy (do usp_SyncOTActualHours làm cả 2 việc trong
    /// 1 lần EXEC). KHÔNG đặt tên/dùng interface này như 1 nguồn dữ liệu an toàn để gọi
    /// lặp lại tùy ý (auto-refresh, OnInitializedAsync...).
    /// </summary>
    public interface IOTAttendanceReconciliationService
    {
        Task<OTReconciliationResultDto> ReconcileActualHoursAsync(
            DateTime date, string? deptCode, CancellationToken ct = default);
    }
}
