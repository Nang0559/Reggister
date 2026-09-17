
namespace FVN_REGISTER.Contract.Dtos.Employees
{
    /// <summary>
    /// Node bộ phận trong cây nhân viên
    /// </summary>
    public class EmployeeDeptTreeDto
    {
        public string DeptCode { get; set; } = "";
        public string DeptName { get; set; } = "";
        public bool IsExpanded { get; set; } = true;
        public List<EmployeeCardDto> Employees { get; set; } = new();
        public int TotalActive => Employees.Count(e => e.IsActive);
        public int Total => Employees.Count;
    }
}
