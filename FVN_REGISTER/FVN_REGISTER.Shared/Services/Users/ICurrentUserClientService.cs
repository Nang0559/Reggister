using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Shared.Services.Users
{
    public interface ICurrentUserClientService
    {
        UserSessionDto? User { get; }
        bool IsLoggedIn => User != null;
        int? PermissionCode => User?.PermissionCode;

        // Khởi tạo dữ liệu từ Token có sẵn trong máy
        Task InitializeAsync(string? explicitToken = null);

        // Xóa dữ liệu khi Logout
        void ClearUser();
    }
}
