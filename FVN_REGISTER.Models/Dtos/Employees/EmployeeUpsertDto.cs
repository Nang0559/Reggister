

namespace FVN_REGISTER.Contract.Dtos.Employees
{
    public class EmployeeUpsertDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string DeptCode { get; set; } = "";
        public string? PositionCode { get; set; }
        public string EmailAddress { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? GenderCode { get; set; }
        public DateTime? FirstWorkingDate { get; set; }
        public DateTime? EndWorkingDate { get; set; }
        public decimal TotalLeaveDays { get; set; }
        public bool IsActive { get; set; } = true;
        public int? LevelApprove { get; set; }
        public int? EmployeeNo { get; set; }
    }
}
