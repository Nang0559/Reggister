using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Security;

namespace FVN_REGISTER.Application.Interfaces.Security;

public interface IAuthorizationService
{
    Task<bool> HasAsync(UserIdentityDto user, int functionCode, CancellationToken ct = default);
    Task<PermissionSnapshotDto> GetSnapshotAsync(int userId, CancellationToken ct = default);
    Task<List<SecurityRoleDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<SecurityFunctionDto>> GetFunctionsAsync(CancellationToken ct = default);
    Task<PermissionSnapshotDto> SetUserRolesAsync(int userId, IReadOnlyCollection<int> roleCodes, int actorUserId, CancellationToken ct = default);
    Task<SecurityRoleDto> SetRoleFunctionsAsync(int roleCode, IReadOnlyCollection<int> functionCodes, int actorUserId, CancellationToken ct = default);
}
