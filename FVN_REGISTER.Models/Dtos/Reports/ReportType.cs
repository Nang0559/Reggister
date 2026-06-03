using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Reports
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
        OvertimeSummary,
        OvertimeByEmployee,
        OvertimeCost,

        // Chấm công (mở rộng sau)
        AttendanceSummary,
        AttendanceDetail,
    }
}
