

namespace FVN_REGISTER.Contract.ViewModels
{
    public class PendingApprovalGroup
    {
        public int ApproverLevel { get; set; }
        public bool Level1IsApprove { get; set; }
        public bool Level2IsApprove { get; set; }
        public bool Level3IsApprove { get; set; }
        public int Level1PendingCount { get; set; }
        public int Level2PendingCount { get; set; }
        public int Level3PendingCount { get; set; }
        public List<LeaveDaysViewModel> Requests { get; set; } = new List<LeaveDaysViewModel>(); // Initialize the list
        public int Count { get; set; }
    }
}
