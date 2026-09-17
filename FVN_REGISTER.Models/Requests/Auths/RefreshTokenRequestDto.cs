using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Auths
{
    public sealed class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token không được để trống")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
