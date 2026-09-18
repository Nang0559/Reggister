using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Auths;

public sealed class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [MaxLength(16, ErrorMessage = "Mật khẩu không quá 16 ký tự.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự.")]
    [MaxLength(16, ErrorMessage = "Mật khẩu không quá 16 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;
}
