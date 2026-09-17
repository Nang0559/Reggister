using FVN_REGISTER.Contract.Dtos.Authentication;

namespace FVN_REGISTER.Shared.Services.Users
{
    public interface ICurrentUserClientService
    {
        UserIdentityDto? User { get; }
        bool IsLoggedIn => User != null;
        int? PermissionCode => User?.PermissionCode;

        Task InitializeAsync(string? explicitToken = null);
        void ClearUser();
    }
}
