

namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows
{
    public class HrmEmployeeSourceRow
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public string? PositionCode { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? GenderCode { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? FirstWorkingDate { get; set; }
    public DateTime? EndWorkingDate { get; set; }
    public decimal? TotalLeaveDays { get; set; }
    public int? EmployeeNo { get; set; }
}
}
