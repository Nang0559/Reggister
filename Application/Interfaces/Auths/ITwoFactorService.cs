using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Auths;

public interface ITwoFactorService
{
    Task<bool> IsTwoFactorRequiredAsync(int userId, CancellationToken ct = default);
    Task<bool> IsSensitiveUserAsync(int userId, CancellationToken ct = default);
    Task<ServiceResult> SetRequiredAsync(int targetUserId, bool required, int actorUserId, CancellationToken ct = default);
    Task<string> CreateLoginChallengeAsync(int userId, CancellationToken ct = default);
    Task<ServiceResult<int>> ValidateLoginChallengeAsync(string challengeToken, string code, CancellationToken ct = default);
    Task<ServiceResult<TwoFactorSetupDto>> BeginSetupAsync(int userId, CancellationToken ct = default);
    Task<ServiceResult<TwoFactorSetupDto>> BeginSetupFromChallengeAsync(string challengeToken, CancellationToken ct = default);
    Task<ServiceResult<int>> ConfirmSetupFromChallengeAsync(string challengeToken, string code, CancellationToken ct = default);
    Task<ServiceResult> ConfirmSetupAsync(int userId, string code, CancellationToken ct = default);
    Task<ServiceResult<TwoFactorStatusDto>> GetStatusAsync(int userId, CancellationToken ct = default);
    Task<ServiceResult> ResetAsync(int targetUserId, int actorUserId, CancellationToken ct = default);
}
