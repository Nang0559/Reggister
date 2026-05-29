
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Nhập mật khẩu hiện tại")]
        [Display(Name = "Mật khẩu hiện tại (*)")]
        public string CurrentPassword { get; set; }


        [Required(ErrorMessage = "Nhập mật khẩu mới")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Độ dài mật khẩu phải từ 8 đến 30 Ký tự")]
        [Display(Name = "Mật khẩu mới (*)")]
        public string NewPassword { get; set; }


        [Required(ErrorMessage = "Nhập lại mật khẩu mới")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Độ dài mật khẩu phải từ 8 đến 30 Ký tự")]
        //   [MaxLength(30, ErrorMessage = "Độ dài mật khẩu mới không quá 30 Ký tự")]
        [Display(Name = "Nhập lại mật khẩu mới (*)")]
        public string NewPassword2 { get; set; }

        public ChangePasswordViewModel()
        {
            CurrentPassword = String.Empty;
            NewPassword = String.Empty;
            NewPassword2 = String.Empty;
        }
    }
}
