using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03TwoFactorChallenges")]
public sealed class F03TwoFactorChallenge : BaseAuditEntity
{
    public int UserId { get; set; }
    [Required, StringLength(128)] public string ChallengeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsConsumed { get; set; }
    public DateTime? ConsumedAt { get; set; }
    [StringLength(20)] public string Purpose { get; set; } = "Login";
    [ForeignKey(nameof(UserId))] public F03User User { get; set; } = null!;
}
