using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests
{
    public class UpdateProfileCommandDto
    {
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(15, ErrorMessage = "Số điện thoại quá dài")]
        public string? PhoneNumber { get; set; }
    }
}
