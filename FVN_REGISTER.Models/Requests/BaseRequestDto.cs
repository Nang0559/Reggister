
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;



namespace FVN_REGISTER.Contract.Requests
{
    // Lớp cơ sở cho toàn bộ đơn (Header)
    public abstract class BaseRequestDto<TDetail> : IHasAttachments
    {
        public int Id { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Description { get; set; } = string.Empty;

        public string RequesterCode { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;

        public List<ApprovalStepCalculatedDto> ApprovalSteps { get; set; } = new();
        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<TDetail> Details { get; set; } = new();

        public string RequesterName { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;

        public ApprovalStatus RequestStatus { get; set; } = ApprovalStatus.Draft;

        // Không còn UIStyle ở đây
        public string StatusText => RequestStatus.ToDisplayName();   
        public bool CanEdit => RequestStatus.CanBeEdited();          
        public bool IsFinished => RequestStatus.IsFinished();        

        public string? GlobalWarning { get; set; }
    }
}
