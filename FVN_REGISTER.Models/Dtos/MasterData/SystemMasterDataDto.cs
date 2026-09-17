using FVN_REGISTER.Contract.Dtos.Approvals;


namespace FVN_REGISTER.Contract.Dtos.MasterData
{
    public class SystemMasterDataDto
    {
        public List<CalendarEventDto> CompanyHolidays { get; set; } = new();
        public List<string> HolidaysNotCountLeave { get; set; } = new();
        public List<WorkYearDto> FiscalYears { get; set; } = new();
        public List<ApprovalStepDto> DefaultApprovalFlow { get; set; } = new();
    }
}
