

namespace FVN_REGISTER.Contract.Dtos.Employees
{
    /// <summary>
    /// Dữ liệu nhân viên hiển thị trong tree/danh sách.
    /// Không chứa property UI (màu sắc, avatar rút gọn, text trạng thái) —
    /// các phần đó chuyển sang extension method ở tầng Shared.
    /// </summary>
    public class EmployeeCardDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string DeptCode { get; set; } = "";
        public string DeptName { get; set; } = "";
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public string EmailAddress { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? GenderName { get; set; }
        public DateTime? FirstWorkingDate { get; set; }
        public DateTime? EndWorkingDate { get; set; }
        public bool IsActive { get; set; }
        public int? LevelApprove { get; set; }

        // Phép tồn — đổi tên khớp VF03LeaveBalance
        public decimal TotalEntitledLeave { get; set; }   // trước: TongPhep
        public decimal LeaveDaysUsed { get; set; }          // trước: SoNgayDaNghi
        public decimal RemainingLeave { get; set; }

        // OT (giờ từ đầu năm đến hiện tại)
        public decimal OtHoursThisYear { get; set; }
        public int OtDaysThisYear { get; set; }

        /// <summary>Số năm công tác — dữ liệu nghiệp vụ tính toán, không phải UI, giữ tại Contract.</summary>
        public int WorkingYears => FirstWorkingDate.HasValue
            ? (int)((DateTime.Now - FirstWorkingDate.Value).TotalDays / 365)
            : 0;
    }
}
