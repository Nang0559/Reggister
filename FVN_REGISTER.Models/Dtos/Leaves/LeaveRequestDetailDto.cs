


using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;

namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    public class LeaveRequestDetailDto : IValidatableRow
    {
        // ── Dữ liệu nghiệp vụ ────────────────────────
        public DateTime LeaveDate { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string? LeaveTypeName { get; set; }
        public bool IsCountedAsLeave { get; set; }
        public bool IsHalfDay { get; set; }
        public HalfDayType? HalfDayOption { get; set; }
        public decimal DayValue { get; set; }

        // ── Implement IValidatableRow ───────────────
        // Thay vì dùng bool HasError, ta dùng Enum Status để quản lý đa dạng trạng thái
        public RowStatus Status { get; set; } = RowStatus.Default;
        public string? Message { get; set; }

       
    }
}
