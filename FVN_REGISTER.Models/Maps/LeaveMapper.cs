using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Maps
{
    public static class LeaveMapper
    {
        // 1. Map từ View Detail sang ViewModel chi tiết của dòng đó
        public static LeaveDetailViewModel ToViewModel(VF03leaveDayDetail d) => new()
        {
            // Vì LeaveDate trong VF03leaveDayDetail là DateOnly, dùng ToDateTime để khớp ViewModel
            LeaveDate = d.LeaveDate.ToDateTime(TimeOnly.MinValue),
            LeaveTypeCode = d.LeaveTypeCode,
            LeaveTypeName = d.LeaveTypeName,
            DayValue = d.DayValue,
            IsHalfDay = d.IsHalfDay,
            HalfDayOption = d.HalfDayOption
        };

        // 2. Map từ danh sách các dòng View (cùng một LeaveId) sang ViewModel tổng thể
        public static LeaveDaysViewModel ToViewModel(List<VF03leaveDayDetail> details)
        {
            var first = details.FirstOrDefault();
            if (first == null) return new LeaveDaysViewModel();

            return new LeaveDaysViewModel
            {
                // Khớp các field từ View FLAT
                Id = first.LeaveId,
                WorkYear = first.WorkYear,
                EmployeeCode = first.EmployeeCode,
                EmployeeName = first.EmployeeName,
                DeptCode = first.DeptCode,
                StartDate = first.StartDate,
                EndDate = first.EndDate,
                RequestStatus = first.RequestStatus,
                TotalDay = details.Sum(x => x.DayValue), // Tính tổng ngày từ các dòng detail

                // Map danh sách detail
                LeaveDetails = [.. details.Select(ToViewModel)],

                LeaveTypeCode = first.LeaveTypeCode,
                LeaveTypeName = first.LeaveTypeName
            };
        }

        // 3. Map từ một dòng View duy nhất sang ViewModel (Dùng cho danh sách rút gọn)
        public static LeaveDaysViewModel FromDetail(VF03leaveDayDetail d) => new()
        {
            Id = d.LeaveId, // Khớp LeaveId thay vì LeaveDaysId
            EmployeeCode = d.EmployeeCode,
            EmployeeName = d.EmployeeName,
            DeptCode = d.DeptCode,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            RequestStatus = d.RequestStatus,
            LeaveTypeCode = d.LeaveTypeCode,
            LeaveTypeName = d.LeaveTypeName,
            LeaveDetails = [ToViewModel(d)]
        };
    }
}
