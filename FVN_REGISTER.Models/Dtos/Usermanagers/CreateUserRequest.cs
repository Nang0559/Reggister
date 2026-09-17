using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Dtos.Usermanagers
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Nhập mã nhân viên")]
        [MaxLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;   // vừa là khóa nghiệp vụ, vừa là đăng nhập

        [Required(ErrorMessage = "Nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public string? DeptCode { get; set; }

        [Required(ErrorMessage = "Chọn quyền")]
        public int PermissionCode { get; set; }

        public List<int>? FunctionIds { get; set; }
    }
}
