
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels
{
    public class UserAccountViewModel
    {
        public int IdUser { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string EmployeeCode { get; set; }

        public string DeptCode { get; set; }

        [Required]
        public string Password { get; set; }

        // ✅ MULTI FUNCTION
        [Required(ErrorMessage = "Chọn chức năng")]
        public List<int> FunctionIds { get; set; } = new List<int>();

        // ✅ CHỈ GIỮ 1 cái
        [Required(ErrorMessage = "Chọn quyền")]
        public int PermissionCode { get; set; }

        public bool IsActive { get; set; }

        public string NewPassword { get; set; } = "";
    }
}
