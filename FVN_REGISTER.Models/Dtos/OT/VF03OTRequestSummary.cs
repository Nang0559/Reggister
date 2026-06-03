using FVN_REGISTER.Contract.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class VF03OTRequestSummary
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? DeptName { get; set; }
        public DateOnly OTDate { get; set; }
        public string DayType { get; set; } = "Normal";
        public decimal PlannedHours { get; set; }
        public string OTReason { get; set; } = null!;
        public bool RequiresGM { get; set; }
        public string RequestStatus { get; set; } = "Pending";
        public string StatusDisplay => OTStatus.GetDisplayName(RequestStatus);
        public string StatusColor => OTStatus.GetColor(RequestStatus);
        public DateTime CreatedAt { get; set; }
        // Người đang chờ duyệt tiếp theo
        public string? NextApproverName { get; set; }
    }
}
