using FVN_REGISTER.Contract.Dtos.LeaveTypes;


namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    public class LeaveConfigDto
    {
        // Chứa danh mục loại phép (Thay thế cho F03leaveType)
        public List<LeaveTypeDto> LeaveTypes { get; set; } = new();

        // Các cấu hình đặc thù của Leave
        public bool AllowPartialDayLeave { get; set; }
        public int MinimumAdvanceNoticeDays { get; set; }
    }
}
