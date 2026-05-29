using FVN_REGISTER.Contract.Models;
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveDaysViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chọn năm")]
        [Display(Name = "Năm (*)")]
        public int WorkYear { get; set; } = DateTime.Now.Year;

        public string EmployeeCode { get; set; }

        [Required(ErrorMessage = "Chọn Ngày viết đơn")]
        [Display(Name = "Ngày viết đơn (*)")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime RegisterDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Chọn ngày bắt đầu nghỉ")]
        [Display(Name = "Nghỉ từ ngày (*)")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Chọn ngày kết thúc nghỉ")]
        [Display(Name = "Nghỉ đến ngày (*)")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Range(0.5, 365)]
        [Display(Name = "Số ngày nghỉ phép (*)")]
        public decimal TotalLeaveDay { get; set; } = 1;

        [Required]
        [Range(0, 365)]
        [Display(Name = "Tổng số ngày nghỉ (*)")]
        public decimal TotalDay { get; set; } = 1;

        [Required(ErrorMessage = "Chọn Hình thức nghỉ")]
        [Display(Name = "Hình thức nghỉ (*)")]
        public string LeaveTypeCode { get; set; } = "0021";

        public string LeaveTypeName { get; set; }

        public List<F03leaveType> LeaveTypes { get; set; } = new List<F03leaveType>();

        [Required]
        [StringLength(500)]
        [Display(Name = "Lý do nghỉ (*)")]
        public string LeaveReason { get; set; } = "Lý do nghỉ";

        public List<LeaveDetailViewModel> LeaveDetails { get; set; } = new List<LeaveDetailViewModel>();
        public List<F03leaveDaysAttachment>? Attachments { get; set; }

        public string RequestStatus { get; set; } = "Pending";

        // Approve
        public string? Level1ApproveCode { get; set; }
        public string? Level1ApproveName { get; set; }
        public string? Level1ApproveEmail { get; set; }

        public string? Level2ApproveCode { get; set; }
        public string? Level2ApproveName { get; set; }
        public string? Level2ApproveEmail { get; set; }

        public bool Level1IsApprove { get; set; }
        public bool Level2IsApprove { get; set; }

        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public string? Level3ApproveEmail { get; set; }
        public bool Level3IsApprove { get; set; }

        public decimal? tongphep { get; set; }
        public decimal Phepton { get; set; }

        public bool IsHalfDay { get; set; }
        public string? HalfDayOption { get; set; }

        // System
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        public string? DeptCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Email { get; set; }

        // ===== HELPER =====
        public void SetLeaveTypeName(List<F03leaveType> leaveTypes)
        {
            var leaveType = leaveTypes.FirstOrDefault(x => x.LeaveTypeCode == LeaveTypeCode);
            if (leaveType != null)
            {
                LeaveTypeName = leaveType.LeaveTypeName;
            }
        }

        public void CalculateTotals()
        {
            if (LeaveDetails == null || !LeaveDetails.Any())
                return;

            TotalDay = LeaveDetails.Sum(x => x.DayValue);
            TotalLeaveDay = LeaveDetails
                .Where(x => x.TinhPhep == 1)
                .Sum(x => x.DayValue);

            LeaveTypeCode = LeaveDetails.FirstOrDefault()?.LeaveTypeCode ?? LeaveTypeCode;
        }
    }
}
