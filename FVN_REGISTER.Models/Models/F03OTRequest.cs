using FVN_REGISTER.Contract.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTRequest
    {
        public int Id { get; set; }
        public string OTCode { get; set; } = null!;
        public string DeptCode { get; set; } = null!;
        public DateOnly OTDate { get; set; }
        public string OTType { get; set; } = "WEEKDAY";         // WEEKDAY | WEEKEND | HOLIDAY
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal PlannedHours { get; set; }
        public string OTReason { get; set; } = null!;
        public string ScopeType { get; set; } = "SELECTED";     // DEPT | SELECTED

        // Level 1 - Sub-leader/Leader (bắt buộc với CVCode 0003)
        public string? Level1ApproveCode { get; set; }
        public string? Level1ApproveName { get; set; }
        public string? Level1ApproveEmail { get; set; }
        public bool? Level1IsApprove { get; set; }
        public DateTime? Level1ApproveTime { get; set; }
        public string? Level1Comment { get; set; }

        // Level 2 - Ast.Chief / Chief
        public string? Level2ApproveCode { get; set; }
        public string? Level2ApproveName { get; set; }
        public string? Level2ApproveEmail { get; set; }
        public bool? Level2IsApprove { get; set; }
        public DateTime? Level2ApproveTime { get; set; }
        public string? Level2Comment { get; set; }

        // Level 3 - A.MG / MG
        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public string? Level3ApproveEmail { get; set; }
        public bool? Level3IsApprove { get; set; }
        public DateTime? Level3ApproveTime { get; set; }
        public string? Level3Comment { get; set; }

        // Level 4 - GM (ngày thường 3 bộ đưa / ngày nhất định)
        public string? Level4ApproveCode { get; set; }
        public string? Level4ApproveName { get; set; }
        public string? Level4ApproveEmail { get; set; }
        public bool? Level4IsApprove { get; set; }
        public DateTime? Level4ApproveTime { get; set; }
        public string? Level4Comment { get; set; }

        public string RequestStatus { get; set; } = OTStatus.Pending;

        // Validate + Archive
        public DateTime? ValidatedAt { get; set; }
        public int? ValidatedBy { get; set; }
        public string? ValidationNote { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public int? ArchivedBy { get; set; }

        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; } = -1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ModifiedBy { get; set; } = -1;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<F03OTEmployee> OTEmployees { get; set; } = new List<F03OTEmployee>();
    }
}
