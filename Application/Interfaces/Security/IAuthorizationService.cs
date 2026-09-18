using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Security;

namespace FVN_REGISTER.Application.Interfaces.Security;

public interface IAuthorizationService
{
    Task<bool> HasAsync(UserIdentityDto user, int functionCode, CancellationToken ct = default);
    Task<PermissionSnapshotDto> GetSnapshotAsync(int userId, CancellationToken ct = default);
}
