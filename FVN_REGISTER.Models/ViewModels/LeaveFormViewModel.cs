

namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveFormViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDay { get; set; }
        public int TotalLeaveDay { get; set; }
        public List<LeaveDetailViewModel> LeaveDetails { get; set; }
    }
}
