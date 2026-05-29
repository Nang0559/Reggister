

namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveStatisticsViewModel
    {
        public LeaveStatisticsViewModel()
        {
            EmployeeLeaves = new List<LeaveDaysViewModel>();
        }
        public int PresentEmployeesCount { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalPresentToday { get; set; }
        public int ApprovedLeaveCount { get; set; }
        public int PendingLeaveCount { get; set; }
        public double LeaveRate { get; set; }
        public double WorkingRate { get; set; }
        public List<LeaveDaysViewModel> EmployeeLeaves { get; set; }
        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>(); //
        public int TotalLeaveCount { get; set; } // Thêm thuộc tính này
    }
}
