using FVN_REGISTER.Contract.Dtos.OT;

namespace FVN_REGISTER.Contract.Dtos.Notifications
{
    public class OTDetailPayload
    {
        public string? OTCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? DeptName { get; set; }
        public DateOnly OTDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalOTHours { get; set; }
        public string? OTTypeName { get; set; }
        public string? OTReasonSummary { get; set; }
        public List<OTEmployeeDto> Employees { get; set; } = new();
    }
}
