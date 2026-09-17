using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Usermanagers
{
    public class UserAccountDto
    {
        public int IdUser { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? DeptCode { get; set; }
        public string? DeptName { get; set; }
        public int PermissionCode { get; set; }
        public string? PermissionName { get; set; }
        public int LevelApprove { get; set; }
        public string? Cvcode { get; set; }
        public bool IsActive { get; set; }
        public bool LockoutEnable { get; set; }
        public DateTime? LockoutEndDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public int NumLoginFailed { get; set; }
        public string? Avatar { get; set; }
        public List<int> FunctionIds { get; set; } = new();

        public bool IsCurrentlyLocked => LockoutEndDate.HasValue && LockoutEndDate.Value > DateTime.Now;
    }
}
