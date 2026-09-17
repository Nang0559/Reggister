
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Requests.Users
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Nhập mật khẩu hiện tại")]
        [Display(Name = "Mật khẩu hiện tại (*)")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập mật khẩu mới")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Độ dài mật khẩu phải từ 8 đến 30 Ký tự")]
        [Display(Name = "Mật khẩu mới (*)")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập lại mật khẩu mới")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Độ dài mật khẩu phải từ 8 đến 30 Ký tự")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu mới và xác nhận không khớp")]
        [Display(Name = "Nhập lại mật khẩu mới (*)")]
        public string NewPassword2 { get; set; } = string.Empty;
    }
}
