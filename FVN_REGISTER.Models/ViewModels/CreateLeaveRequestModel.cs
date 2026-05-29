
using FVN_REGISTER.Contract.Models;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class CreateLeaveRequestModel
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }

        public string? LeaveReason { get; set; }

        public int WorkYear { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<CreateLeaveDetailModel>? Details { get; set; }

        public decimal TotalDay { get; set; }

        public decimal TotalLeaveDay { get; set; }

        // 🔥 ADD
        public List<F03leaveType> LeaveTypes { get; set; } = new();
        public string? Level1ApproveEmail { get; set; }
        public string? Level2ApproveEmail { get; set; }
        public string? Level3ApproveEmail { get; set; }
        public string? Level1ApproveCode { get; set; }
        public string? Level2ApproveCode { get; set; }
        public string? Level3ApproveCode { get; set; }

        // 🔥 ADD (global setting)
        public bool IsHalfDay { get; set; }
        public string? HalfDayOption { get; set; }

        public void CalculateTotals()
        {
            if (Details == null || !Details.Any())
            {
                TotalDay = 0;
                TotalLeaveDay = 0;
                return;
            }

            TotalDay = Details.Sum(x => x.DayValue);

            TotalLeaveDay = Details
                .Where(x => x.TinhPhep == 1)
                .Sum(x => x.DayValue);
        }
    }
}
