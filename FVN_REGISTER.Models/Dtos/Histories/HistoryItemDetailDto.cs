using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Notifications;


namespace FVN_REGISTER.Contract.Dtos.Histories
{
    public class HistoryItemDetailDto
    {
        // Header chung
        public int Id { get; set; }
        public string Kind { get; set; } = "";
        public string RequestStatus { get; set; } = "";
        public string StatusDisplay { get; set; } = "";
        public string StatusColor { get; set; } = "";
        public DateTime SubmittedAt { get; set; }
        public bool CanCancel { get; set; }

        // Leave detail
        public LeaveDetailPayload? Leave { get; set; }

        // OT detail
        public OTDetailPayload? OT { get; set; }

        public TripDetailPayload? Trip { get; set; }
        public EquipmentDetailPayload? Equipment { get; set; }

        // Approval timeline — dùng chung
        public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();
    }
}
