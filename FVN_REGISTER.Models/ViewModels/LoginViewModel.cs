using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tài khoản")]
        public String UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [MaxLength(16, ErrorMessage = "Mật khẩu không quá 16 ký tự")]
        public String Password { get; set; }

        
    }
}
