using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.HRM
{
    public partial class HrmLeaveTypeChangeLog
    {
        public long Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string? LeaveTypeName { get; set; }
        public string? LeaveTypeName2 { get; set; }
        public bool? TinhPhep { get; set; }
        public string? HRMCode { get; set; }
        public HrmChangeAction ActionType { get; set; }
        public DateTime ChangedAt { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
