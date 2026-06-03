using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class VF03OTRequest
    {
        public int Id { get; set; }
        public int WorkYear { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string DeptCode { get; set; } = null!;
        public string? DeptName { get; set; }
        public string? CvCode { get; set; }
        public string? CvName { get; set; }
        public string? EmailAddress { get; set; }

        public DateOnly OTDate { get; set; }
        public DateTime PlannedFrom { get; set; }
        public DateTime PlannedTo { get; set; }
        public decimal PlannedHours { get; set; }
        public string DayType { get; set; } = "Normal";
        public string OTReason { get; set; } = null!;
        public bool RequiresGM { get; set; }
        public string? CreatedByEmail { get; set; }
        public string? CreatedByLevel { get; set; }

        // Lv3
        public string? Lv3ApproveName { get; set; }
        public string? Lv3ApproveEmail { get; set; }
        public bool? Lv3IsApprove { get; set; }
        public DateTime? Lv3ApproveTime { get; set; }
        public string? Lv3Comment { get; set; }
        public bool Lv3Skip { get; set; }
        public string? Lv3StatusText { get; set; }

        // Lv4
        public string? Lv4ApproveName { get; set; }
        public string? Lv4ApproveEmail { get; set; }
        public bool? Lv4IsApprove { get; set; }
        public DateTime? Lv4ApproveTime { get; set; }
        public string? Lv4Comment { get; set; }
        public string? Lv4StatusText { get; set; }

        // Lv5
        public string? Lv5ApproveName { get; set; }
        public string? Lv5ApproveEmail { get; set; }
        public bool? Lv5IsApprove { get; set; }
        public DateTime? Lv5ApproveTime { get; set; }
        public string? Lv5Comment { get; set; }
        public string? Lv5StatusText { get; set; }

        // Lv6
        public string? Lv6ApproveName { get; set; }
        public string? Lv6ApproveEmail { get; set; }
        public bool? Lv6IsApprove { get; set; }
        public DateTime? Lv6ApproveTime { get; set; }
        public string? Lv6Comment { get; set; }
        public string? Lv6StatusText { get; set; }

        // Lv7 (GM)
        public string? Lv7ApproveName { get; set; }
        public string? Lv7ApproveEmail { get; set; }
        public bool? Lv7IsApprove { get; set; }
        public DateTime? Lv7ApproveTime { get; set; }
        public string? Lv7Comment { get; set; }
        public string? Lv7StatusText { get; set; }

        public string RequestStatus { get; set; } = "Pending";
        public DateTime? ActualFrom { get; set; }
        public DateTime? ActualTo { get; set; }
        public decimal? ActualHours { get; set; }
        public bool EmployeeConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
