using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{


    // View: vF03OTRequest
    // Join F03OTRequest + F03OTEmployee (aggregate) + F03OTApprover
    public partial class VF03OTRequest
    {
        public int? Id { get; set; }

        public string? OTCode { get; set; }

        public string? EmployeeCode { get; set; }       // người tạo đơn

        public string? EmployeeName { get; set; }

        public string? CreatedByEmail { get; set; }

        public string? DeptCode { get; set; }

        public string? DeptName { get; set; }

        public string? ScopeType { get; set; }

        public DateTime OTDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public decimal? TotalOTHours { get; set; }

        public string? OTTypeCode { get; set; }

        public string? OTTypeName { get; set; }         // computed trong view

        public string? Reason { get; set; }

        public string? RequestStatus { get; set; }

        public int? EmployeeCount { get; set; }         // COUNT từ F03OTEmployee

        // ===== Level 3 =====
        public string? Level3ApproveEmail { get; set; }
        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public bool? Level3IsApprove { get; set; }
        public DateTime? Level3ApproveTime { get; set; }
        public string? Level3Comment { get; set; }

        // ===== Level 5 =====
        public string? Level5ApproveEmail { get; set; }
        public string? Level5ApproveCode { get; set; }
        public string? Level5ApproveName { get; set; }
        public bool? Level5IsApprove { get; set; }
        public DateTime? Level5ApproveTime { get; set; }
        public string? Level5Comment { get; set; }

        // ===== Level 6 =====
        public string? Level6ApproveEmail { get; set; }
        public string? Level6ApproveCode { get; set; }
        public string? Level6ApproveName { get; set; }
        public bool? Level6IsApprove { get; set; }
        public DateTime? Level6ApproveTime { get; set; }
        public string? Level6Comment { get; set; }

        // ===== Level 7 =====
        public string? Level7ApproveEmail { get; set; }
        public string? Level7ApproveCode { get; set; }
        public string? Level7ApproveName { get; set; }
        public bool? Level7IsApprove { get; set; }
        public DateTime? Level7ApproveTime { get; set; }
        public string? Level7Comment { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
