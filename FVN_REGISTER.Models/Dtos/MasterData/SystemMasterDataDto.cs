using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Contract.Dtos.Leaves;

namespace FVN_REGISTER.Contract.Dtos.MasterData
{
    /// <summary>
    /// Read-only data required by the leave registration UI.
    /// Contains contract DTOs only; no persistence or UI ViewModels.
    /// </summary>
    public class SystemMasterDataDto
    {
        public List<CalendarEventDto> CompanyHolidays { get; set; } = new();
        public List<string> HolidaysNotCountLeave { get; set; } = new();
        public List<WorkYearDto> FiscalYears { get; set; } = new();
        public List<LeaveTypeDto> LeaveTypes { get; set; } = new();
        public List<LeaveCalendarEventDto> LeaveEvents { get; set; } = new();
        public List<ApprovalStepDto> DefaultApprovalFlow { get; set; } = new();
    }
}
