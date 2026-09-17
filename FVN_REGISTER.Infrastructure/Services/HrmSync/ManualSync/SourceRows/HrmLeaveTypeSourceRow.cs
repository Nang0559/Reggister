namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows
{
    /// <summary>
    /// DTO thô 1-1 với cột nguồn HRM cho Loại nghỉ phép — chỉ tồn tại ở Infrastructure,
    /// không lộ ra ngoài. Field khớp đúng những gì F03StagingLeaveType/F03LeaveType cần
    /// (EntityKey, LeaveTypeName, LeaveTypeName2, HRMCode) — KHÔNG có TinhPhep vì đây là
    /// field business tự cấu hình ở FVN, HrmSyncJob không đụng vào (xem LeaveTypeHrmSyncJob).
    /// </summary>
    public class HrmLeaveTypeSourceRow
    {
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string? LeaveTypeName2 { get; set; }
        public string? HRMCode { get; set; }
    }
}
