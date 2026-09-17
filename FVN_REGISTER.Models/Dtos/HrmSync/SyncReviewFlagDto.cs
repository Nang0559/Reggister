

namespace FVN_REGISTER.Contract.Dtos.HrmSync
{
    public class SyncReviewFlagDto
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityKey { get; set; } = string.Empty;
        public string FlagType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }
}
