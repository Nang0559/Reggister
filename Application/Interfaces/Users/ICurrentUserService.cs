using FVN_REGISTER.Contract.Dtos.Authentication;

namespace FVN_REGISTER.Application.Interfaces.Users;

public interface ICurrentUserService : FVN_REGISTER.Application.Interfaces.Security.ICurrentUserService
{
    UserIdentityDto? GetCurrentUser();
    bool IsLoggedIn { get; }
    string? GetUserId();
}
