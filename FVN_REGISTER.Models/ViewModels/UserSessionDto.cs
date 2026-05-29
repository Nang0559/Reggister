

namespace FVN_REGISTER.Contract.ViewModels
{
    public class UserSessionDto
    {
        // Thông tin định danh
        public bool IsLoggedIn { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }

        // Thông tin phân quyền & tổ chức
        public int PermissionCode { get; set; }
        public string? DeptCode { get; set; }
        public string? CVCode { get; set; }
        public int? LevelApprove { get; set; }
        public bool IsAdmin { get; set; }
        public bool UseDIC { get; set; }

        // Token để gọi API (Thay thế cho SessionId)
        public string? Token { get; set; }
    }
}
