

namespace FVN_REGISTER.Contract.ViewModels
{
    public class ApprovalLevelViewModel
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int ApprovalCount { get; set; }
        public string EmployeeName { get; set; }
        public string LevelApNames { get; set; }
        public int ApproverLevel { get; set; }
        public bool Level1IsApprove { get; set; }
        public bool Level2IsApprove { get; set; }
        public int Count { get; set; }
        public List<LeaveDaysViewModel> Requests { get; set; }
    }
}
