using FVN_REGISTER.Contract.Dtos.Authentication;


namespace FVN_REGISTER.Application.Interfaces.Users
{
    public interface ICurrentUserService
    {
        // Trả về null nếu không tìm thấy Token hợp lệ
        UserIdentityDto? GetCurrentUser();

        // Các hàm check nhanh dựa trên Claims
        bool IsLoggedIn { get; }
        string? GetUserId(); // Thường trả về ID dưới dạng string từ ClaimTypes.NameIdentifier
    }
}
