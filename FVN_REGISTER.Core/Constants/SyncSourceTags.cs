namespace FVN_REGISTER.Core.Constants
{
    /// <summary>
    /// Nguồn ghi cuối cùng vào 1 entity kế thừa BaseAuditEntity.LastModifiedSource —
    /// dùng bởi HrmSyncJob (ghi Hrm) và CodeKeyedManagementService (ghi Manual)
    /// để phát hiện xung đột "sync đè lên sửa tay".
    /// </summary>
    public static class SyncSourceTags
    {
        public const string Hrm = "HRM";
        public const string Manual = "Manual";
    }
}
