using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Users
{
    public sealed class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
