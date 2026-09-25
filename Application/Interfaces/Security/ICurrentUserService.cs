using FVN_REGISTER.Contract.Dtos.Authentication;

namespace FVN_REGISTER.Application.Interfaces.Security;

/// <summary>
/// Canonical current-user abstraction consumed by authorization/security services.
/// The Users namespace interface inherits this contract for backward compatibility.
/// </summary>
public interface ICurrentUserService
{
    UserIdentityDto? GetCurrentUser();
    bool IsLoggedIn { get; }
    string? GetUserId();
}
