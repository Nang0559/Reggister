using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Requests.Auths
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập Tài khoản")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [MaxLength(16, ErrorMessage = "Mật khẩu không quá 16 ký tự")]
        public string Password { get; set; } = string.Empty;

        public string DeviceId { get; set; } = Guid.NewGuid().ToString();
        public string DeviceType { get; set; } = "Web";
        public string? DeviceName { get; set; }
        public bool RememberMe { get; set; }
    }
}
