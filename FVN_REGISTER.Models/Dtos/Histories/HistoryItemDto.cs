using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Enums;



namespace FVN_REGISTER.Contract.Dtos.Histories
{
    public class HistoryItemDto: IHasAttachments
    {
        public int Id { get; set; }
        public RequestModule Kind { get; set; } = RequestModule.Leave;

    

        // Chung
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DeptCode { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public bool CanCancel { get; set; }

        // Leave specific
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TotalDay { get; set; }
        public decimal?  TotalLeaveDay { get; set; }
        public string? LeaveTypeName { get; set; }

        // OT specific
        public DateOnly? OTDate { get; set; }
        public decimal? TotalOTHours { get; set; }
        public string? OTTypeName { get; set; }
        public int? EmployeeCount { get; set; }

        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();
    }
}
