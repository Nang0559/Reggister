

namespace FVN_REGISTER.Contract.ViewModels
{
    public class CreateLeaveDetailModel
    {
        public DateTime LeaveDate { get; set; }

        public string LeaveTypeCode { get; set; }
        public string LeaveTypeName { get; set; }
        public bool IsHalfDay { get; set; }

        public string? HalfDayOption { get; set; } // Morning / Afternoon

        public decimal DayValue { get; set; } // 1 hoặc 0.5

        public int TinhPhep { get; set; } // 🔥 0 hoặc 1
    }
}
