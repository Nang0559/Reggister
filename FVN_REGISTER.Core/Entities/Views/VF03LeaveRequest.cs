using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.Views
{
    // Models/Entities/Views/VF03LeaveRequest.cs
    public partial class VF03LeaveRequest
    {
        public int Id { get; set; }
        // The current SQL view does not expose the legacy LeaveCode column.\n        // Keep the CLR property for DTO/mapping compatibility, but never map/query it through EF.\n        [System.ComponentModel.DataAnnotations.Schema.NotMapped]\n        public string? LeaveCode { get; set; }
        public int? WorkYear { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string? GenderName { get; set; }
        public string? DeptCode { get; set; }
        public string? DeptName { get; set; }
        public string? EmailAddress { get; set; }

        public DateTime RegisterDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalDay { get; set; }
        public decimal? TotalLeaveDay { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }

        public string? LeaveReason { get; set; }
        public ApprovalStatus RequestStatus { get; set; } 

        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public string? AttachedDocuments { get; set; }
    }
}
