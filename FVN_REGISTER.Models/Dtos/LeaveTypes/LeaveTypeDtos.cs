

namespace FVN_REGISTER.Contract.Dtos.LeaveTypes
{
    // Dùng cho danh sách và hiển thị chi tiết (Read-only)
    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string? LeaveTypeName2 { get; set; }
        public bool IsCountedAsLeave { get; set; }
        public string? HRMCode { get; set; }
        public bool IsActive { get; set; }
    }

}
