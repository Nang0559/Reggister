using FVN_REGISTER.Core.Constants;

namespace FVN_REGISTER.Contract.Dtos.Authentication
{
    /// <summary>
    /// Canonical authenticated-user identity contract returned by the profile endpoint.
    /// Token/login transport data belongs to AuthResultDto; this DTO is the client identity model.
    /// </summary>
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

        public bool IsLoggedIn { get; set; }
        public int PermissionCode => Permission;
        public bool IsAdmin => Permission == UserPermissionCodes.SuperAdmin || Permission == UserPermissionCodes.Admin;
    }
}
