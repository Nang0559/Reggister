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

        public string EmployeeCode { get; set; } = null!;

        public string? CreatedByEmail { get; set; }

        public string DeptCode { get; set; } = null!;

        public string? ScopeType { get; set; }          // SELECTED | DEPARTMENT

        public DateTime OTDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public decimal PlannedHours { get; set; }

        public decimal TotalOTHours { get; set; }       // tính sau khi save

        public string OTTypeCode { get; set; } = null!; // WEEKDAY | WEEKEND | HOLIDAY

        public string? OTReason { get; set; }

        public string RequestStatus { get; set; } = null!;

        // ===== Level 3: Sub-leader / Leader =====
        public string? Level3ApproveEmail { get; set; }
        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public bool? Level3IsApprove { get; set; }
        public DateTime? Level3ApproveTime { get; set; }
        public string? Level3Comment { get; set; }

        // ===== Level 5: Ast. Chief / Chief =====
        public string? Level5ApproveEmail { get; set; }
        public string? Level5ApproveCode { get; set; }
        public string? Level5ApproveName { get; set; }
        public bool? Level5IsApprove { get; set; }
        public DateTime? Level5ApproveTime { get; set; }
        public string? Level5Comment { get; set; }

        // ===== Level 6: A.MG / MG =====
        public string? Level6ApproveEmail { get; set; }
        public string? Level6ApproveCode { get; set; }
        public string? Level6ApproveName { get; set; }
        public bool? Level6IsApprove { get; set; }
        public DateTime? Level6ApproveTime { get; set; }
        public string? Level6Comment { get; set; }

        // ===== Level 7: GM =====
        public string? Level7ApproveEmail { get; set; }
        public string? Level7ApproveCode { get; set; }
        public string? Level7ApproveName { get; set; }
        public bool? Level7IsApprove { get; set; }
        public DateTime? Level7ApproveTime { get; set; }
        public string? Level7Comment { get; set; }

        // ===== Audit =====
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // ===== Navigation =====
        public virtual ICollection<F03OTEmployee> F03OTEmployees { get; set; }
            = new List<F03OTEmployee>();
    }
}
