namespace FVN_REGISTER.Contract.Dtos.Leaves;

public sealed class LeaveEntitlementDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int WorkYear { get; set; }
    public DateTime FirstWorkingDate { get; set; }
    public DateTime CalculationDate { get; set; }
    public int YearsOfService { get; set; }
    public decimal BaseLeaveDays { get; set; }
    public decimal SeniorityLeaveDays { get; set; }
    public decimal TotalLeaveDays { get; set; }
}
