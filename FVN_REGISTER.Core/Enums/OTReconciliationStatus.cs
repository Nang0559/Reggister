

namespace FVN_REGISTER.Core.Enums
{
    /// <summary>Trạng thái đối chiếu giữa chấm công thực tế và đơn OT đã đăng ký — khớp cột TinhHuong của usp_SyncOTActualHours.</summary>
    public enum OTReconciliationStatus
    {
        NoRequest,           // CHUA_CO_DON — có chấm công nhưng chưa có đơn OT
        RequestNotApproved,  // DON_CHUA_DUYET — có đơn nhưng chưa duyệt
        PendingConfirm,      // CHUA_CONFIRM — đã có ActualHours ở staging, chưa ghi vào F03OTEmployee
        Processed            // DA_XU_LY — đã ghi ActualHours vào F03OTEmployee
    }

    /// <summary>Mức cảnh báo lệch giờ — khớp cột ValidationStatus của usp_SyncOTActualHours.</summary>
    public enum OTHourValidationStatus
    {
        Valid,
        Warning
    }
    // <summary>Vòng đời chạy của 1 BackgroundService định kỳ — dùng chung cho mọi worker
    /// (OTAttendanceStagingWorker, HrmSyncWorker...), KHÔNG liên quan business status như OTReconciliationStatus.</summary>
    public enum WorkerRunState
    {
        Waiting,
        Running,
        Error,
        Stopped
    }
}
