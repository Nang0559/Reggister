using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Requests.Leaves
{
    public class LeaveRequestDetailUpsertDto
    {
        [Required]
        public DateTime LeaveDate { get; set; }

        [Required, StringLength(10)]
        public string LeaveTypeCode { get; set; } = string.Empty;

        public bool IsHalfDay { get; set; }
        public HalfDayType? HalfDayOption { get; set; }

        // DayValue, IsCountedAsLeave: KHÔNG nhận từ client — server tự tính lại
        // trong Validator dựa trên IsHalfDay + F03LeaveType.TinhPhep, tránh client tự set sai.
    }
}
