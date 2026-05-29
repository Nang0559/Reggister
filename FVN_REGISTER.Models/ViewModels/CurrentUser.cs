

namespace FVN_REGISTER.Contract.ViewModels
{
    public class CurrentUser
    {
        public int UserId { get; set; }
        public int Permission { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DeptCode { get; set; }
        public string? CvCode { get; set; }
        public string? Email { get; set; }
        public int LevelApprove { get; set; }

        public List<int> Functions { get; set; } = new List<int>();
        public bool IsSuperAdmin() => Permission == 1;
        public bool IsAdmin() => Permission == 2;
        public bool IsApprover() => Permission == 4;
        public bool IsUser() => Permission == 5;
        public bool CanRequestLeave() => Permission == 4 || Permission == 5;
    }
}
