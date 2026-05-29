
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveDetailViewModel
    {
        [Required]
        public DateTime LeaveDate { get; set; }

        [Required]
        public string LeaveTypeCode { get; set; }

        public string LeaveTypeName { get; set; }

        // 1 = tính phép, 0 = không tính phép
        public int TinhPhep { get; set; }
        public bool IsHalfDay { get; set; }
        public string HalfDayOption { get; set; } // Morning / Afternoon

        // Số ngày nghỉ: 1 hoặc 0.5
        public decimal DayValue { get; set; } = 1;
    }
}
