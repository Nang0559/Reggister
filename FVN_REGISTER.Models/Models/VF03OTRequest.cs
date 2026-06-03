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
        public string OTCode { get; set; } = null!;
        public string DeptCode { get; set; } = null!;
        public string? DeptName { get; set; }
        public DateOnly OTDate { get; set; }
        public string OTType { get; set; } = null!;
        public string? StartTimeText { get; set; }
        public string? EndTimeText { get; set; }
        public decimal PlannedHours { get; set; }
        public string? OTReason { get; set; }
        public string ScopeType { get; set; } = null!;
        public string RequestStatus { get; set; } = null!;
        public string? StatusText { get; set; }
        public string? StatusColor { get; set; }
        public int EmployeeCount { get; set; }

        public string? Level1ApproveCode { get; set; }
        public string? Level1ApproveName { get; set; }
        public string? Level1ApproveEmail { get; set; }
        public bool? Level1IsApprove { get; set; }
        public DateTime? Level1ApproveTime { get; set; }
        public string? Level1Comment { get; set; }

        public string? Level2ApproveCode { get; set; }
        public string? Level2ApproveName { get; set; }
        public string? Level2ApproveEmail { get; set; }
        public bool? Level2IsApprove { get; set; }
        public DateTime? Level2ApproveTime { get; set; }
        public string? Level2Comment { get; set; }

        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public string? Level3ApproveEmail { get; set; }
        public bool? Level3IsApprove { get; set; }
        public DateTime? Level3ApproveTime { get; set; }
        public string? Level3Comment { get; set; }

        public string? Level4ApproveCode { get; set; }
        public string? Level4ApproveName { get; set; }
        public string? Level4ApproveEmail { get; set; }
        public bool? Level4IsApprove { get; set; }
        public DateTime? Level4ApproveTime { get; set; }
        public string? Level4Comment { get; set; }

        public DateTime? ValidatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
