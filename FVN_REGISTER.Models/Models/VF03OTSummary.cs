using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{


    // View: vF03OTSummary
    // Tổng hợp giờ OT theo nhân viên + năm
    // SELECT EmployeeCode, WorkYear,
    //        SUM(CASE WHEN RequestStatus='Approved' THEN OTHours ELSE 0 END) AS TotalApprovedHours,
    //        SUM(CASE WHEN RequestStatus IN ('Pending','ApprovedLv3','ApprovedLv5','ApprovedLv6')
    //                 THEN OTHours ELSE 0 END) AS TotalPendingHours
    // FROM F03OTEmployee e JOIN F03OTRequest r ON e.OTRequestId = r.Id
    // GROUP BY e.EmployeeCode, r.WorkYear (hoặc YEAR(r.OTDate))
    public partial class VF03OTSummary
    {
        public string EmployeeCode { get; set; } = null!;

        public string? EmployeeName { get; set; }

        public string? DeptCode { get; set; }

        public string? DeptName { get; set; }

        public int WorkYear { get; set; }

        // Tổng giờ OT đã được duyệt hoàn tất (RequestStatus = 'Approved')
        public decimal TotalApprovedHours { get; set; }

        // Tổng giờ OT đang trong luồng duyệt (Pending / ApprovedLv3 / Lv5 / Lv6)
        public decimal TotalPendingHours { get; set; }

        // Tổng cộng (dùng để so sánh với giới hạn năm)
        public decimal TotalHours => TotalApprovedHours + TotalPendingHours;
    }
}
