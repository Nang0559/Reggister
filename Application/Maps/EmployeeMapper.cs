using FVN_REGISTER.Contract.Dtos.Employees;
using FVN_REGISTER.Core.Entities.Views;


namespace FVN_REGISTER.Application.Maps
{
    public static class EmployeeMapper
    {
        public static EmployeeCardDto ToDto(
            this VF03employee e,
            VF03LeaveBalance? balance = null,
            (int TotalDays, double TotalMinutes)? ot = null)
        {
            var otHours = ot != null ? Math.Max(0, (decimal)(ot.Value.TotalMinutes / 60.0)) : 0;

            return new EmployeeCardDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                DeptCode = e.DeptCode ?? "",
                DeptName = e.DeptName ?? "",
                PositionCode = e.Cvcode,
                PositionName = e.Cvname,
                EmailAddress = e.EmailAddress,
                PhoneNumber = e.PhoneNumber,
                BirthDate = e.BirthDate,
                GenderName = e.GenderName,
                FirstWorkingDate = e.FirstWorkingDate,
                EndWorkingDate = e.EndWorkingDate,
                IsActive = e.IsActive,
                // VF03employee view chưa có LevelApprove -> xử lý riêng nếu cần
                TotalEntitledLeave = balance?.TotalEntitledLeave ?? e.TongPhep,
                LeaveDaysUsed = balance?.LeaveDaysUsed ?? 0,
                RemainingLeave = balance?.RemainingLeave ?? 0,   // RemainingLeave là decimal? nên cần ?? 0
                OtHoursThisYear = Math.Round(otHours, 1),
                OtDaysThisYear = ot?.TotalDays ?? 0
            };
        }
    }
}
