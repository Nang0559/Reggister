

namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows
{
    public class HrmEmployeeSourceRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DeptCode { get; set; }
        public string? PositionCode { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? GenderCode { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? FirstWorkingDate { get; set; }

        /// <summary>
        /// NULL nếu nhân viên đang làm việc (HRM lưu ngày sentinel ~9990-12-31,
        /// đã được chuẩn hóa về NULL ngay tại SQL Reader).
        /// </summary>
        public DateTime? EndWorkingDate { get; set; }

        public decimal? TotalLeaveDays { get; set; }

        /// <summary>Suy ra từ EndWorkingDate — computed, không cần HRM trả riêng.</summary>
        public bool IsActive => !EndWorkingDate.HasValue;
    }
}
