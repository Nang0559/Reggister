namespace FVN_REGISTER.Contract.Dtos.Authentication
{
    public class AuthResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }

        // Token
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

        // Thông tin người dùng cơ bản để UI hiển thị
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public bool IsAdmin { get; set; }
    }
}
