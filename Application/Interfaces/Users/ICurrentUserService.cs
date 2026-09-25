using FVN_REGISTER.Contract.Dtos.Authentication;

namespace FVN_REGISTER.Application.Interfaces.Users;

/// <summary>
/// Canonical current-user abstraction used across Application, Infrastructure and API.
/// </summary>
public interface ICurrentUserService
{
    UserIdentityDto? GetCurrentUser();
    bool IsLoggedIn { get; }
    string? GetUserId();
}
