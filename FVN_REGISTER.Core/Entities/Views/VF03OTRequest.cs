using FVN_REGISTER.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Entities.Views
{


    // View: vF03OTRequest
    // Join F03OTRequest + F03OTEmployee (aggregate) + F03OTApprover
    public partial class VF03OTRequest
    {
        public int? Id { get; set; }
        public string? OTCode { get; set; }
        public RequestModule RequestType { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? CreatedByEmail { get; set; }
        public string? DeptCode { get; set; }
        public string? DeptName { get; set; }
        public DateTime OTDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? TotalOTHours { get; set; }
        public string? OTTypeCode { get; set; }
        public string? OTTypeName { get; set; }       // ✅ thêm lại — giờ lấy thật từ F03OTType
        public decimal? RateMultiplier { get; set; }   // ✅ thêm mới
        public string? OTReasonSummary { get; set; }
        public string? ScopeType { get; set; }
        public ApprovalStatus RequestStatus { get; set; }
        public int? EmployeeCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
    }

}
