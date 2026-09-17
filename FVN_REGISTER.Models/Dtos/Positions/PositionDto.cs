

namespace FVN_REGISTER.Contract.Dtos.Positions
{
    public class PositionDto
    {
        public int Id { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public bool IsApprove { get; set; }
        public bool IsAllowApprove { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Số nhân viên đang giữ chức vụ này (tuỳ chọn, chỉ set khi cần hiển thị).</summary>
        public int EmployeeCount { get; set; }
    }
}
