namespace FVN_REGISTER.Core.Enums
{
    public enum ReportType
    {
        // Nghỉ phép (hiện tại)
        LeaveBalance,          // Số dư phép
        LeaveSummaryByDept,    // Tổng hợp theo phòng
        LeaveSummaryByEmployee,// Tổng hợp theo nhân viên
        LeaveDetail,           // Chi tiết từng đơn
        LeaveApprovalStatus,   // Trạng thái phê duyệt

        // Làm thêm (mở rộng sau)
        // ── OT (thêm mới) ─────────────────────
        OTSummaryByDept,
        OTSummaryByEmployee,
        OTDetail,
        OTApprovalStatus,
        OTLimitUsage,

        // Chấm công (mở rộng sau)
        AttendanceSummary,
        AttendanceDetail,

        // Công tác
        TripSummaryByDept,
        TripSummaryByEmployee,
        TripDetail,
        TripApprovalStatus,

        // Thiết bị
        EquipmentSummaryByDept,
        EquipmentAssetDetail,
        EquipmentRepairSummary,

        // Báo cáo quản trị
        CompanyWorkloadSummary,
        SecurityAuditSummary,
    }
}
