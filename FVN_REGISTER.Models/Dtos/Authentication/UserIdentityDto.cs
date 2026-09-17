using FVN_REGISTER.Core.Constants;
using System.Security.Permissions;

namespace FVN_REGISTER.Contract.Dtos.Authentication
{
    public class UserIdentityDto
    {
        public int UserId { get; set; }
        public int Permission { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DeptCode { get; set; }
        public string? PositionCode { get; set; }
        public string? Email { get; set; }
        public int LevelApprove { get; set; }
        public List<int> Functions { get; set; } = new();
       
    }
}
