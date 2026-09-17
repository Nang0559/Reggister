using FVN_REGISTER.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Entities.Views
{
   

    public partial class VF03OTRequestDetail
    {
        public long? RowId { get; set; }
        public int OTRequestId { get; set; }
        public string? OTCode { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? DeptCode { get; set; }
        public string? DeptName { get; set; }
        public int DetailId { get; set; }
        public DateOnly OTDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal OTHours { get; set; }

        // ✅ Bổ sung — khớp đủ với view vF03OTRequestDetails đã thiết kế
        public string? CvCode { get; set; }
        public string? EmpOTTypeCode { get; set; }
        public decimal? OTRateMultiplier { get; set; }
        public decimal? ActualHours { get; set; }
        public TimeSpan? ActualStartTime { get; set; }
        public TimeSpan? ActualEndTime { get; set; }
        public string? ValidationStatus { get; set; }
        public string? ValidationMessage { get; set; }
        public string? Note { get; set; }

        public string? OTReasonCategoryCode { get; set; }
        public string? OTReasonDetail { get; set; }
        public ApprovalStatus RequestStatus { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}
