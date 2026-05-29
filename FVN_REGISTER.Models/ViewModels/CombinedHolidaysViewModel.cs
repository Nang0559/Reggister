


using FVN_REGISTER.Contract.Models;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class CombinedHolidaysViewModel
    {
        public List<HolidayViewModel> CompanyHolidays { get; set; } = new List<HolidayViewModel>();

        public List<string> HolidaysNotCountLeave { get; set; } = new List<string>();

        public List<HolidayViewModel> LeaveDays { get; set; } = new List<HolidayViewModel>();

        public CreateLeaveRequestModel LeaveForm { get; set; } = new CreateLeaveRequestModel();
        public List<F03leaveType> LeaveTypes { get; set; } = new ();

        public List<VF03phepTon> PhepTon { get; set; } = new List<VF03phepTon>();

        public List<F03workYear> WorkYears { get; set; } = new ();

        public int UserLevel { get; set; }

        public List<VF03leaveDaysApprover> LVA1 { get; set; } = new ();
        public List<VF03leaveDaysApprover> LVA2 { get; set; } = new ();
        public List<VF03leaveDaysApprover> LVA3 { get; set; } = new();
    }
}
