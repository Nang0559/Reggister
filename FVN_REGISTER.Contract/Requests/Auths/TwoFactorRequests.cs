using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Auths;

public sealed class TwoFactorSetupRequest
{
    [Required] public string ChallengeToken { get; set; } = string.Empty;
}

public sealed class TwoFactorVerifyRequest
{
    [Required] public string ChallengeToken { get; set; } = string.Empty;
    [Required, StringLength(8, MinimumLength = 6)] public string Code { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public sealed class TwoFactorSetupConfirmRequest
{
    [Required, StringLength(8, MinimumLength = 6)] public string Code { get; set; } = string.Empty;
}
