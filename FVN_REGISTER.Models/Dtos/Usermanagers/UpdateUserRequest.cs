using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Dtos.Usermanagers
{
    public class UpdateUserRequest
    {
        public int IdUser { get; set; }

        public string? DeptCode { get; set; }

        [Required(ErrorMessage = "Chọn quyền")]
        public int PermissionCode { get; set; }

        public string? NewPassword { get; set; }

        public List<int>? FunctionIds { get; set; }
    }
}
