namespace FVN_REGISTER.Core.Enums
{
    public enum ApprovalStatus
    {
        Draft = 0,
        Pending = 1,
        InProgress = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5,
        Escalated = 6,
        NeedsRevision = 7    // thêm cuối, không phá vỡ giá trị cũ
    }
}
