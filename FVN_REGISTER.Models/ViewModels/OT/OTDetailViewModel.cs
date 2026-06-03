using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTDetailViewModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? DeptName { get; set; }
        public DateOnly OTDate { get; set; }
        public string DayType { get; set; } = "Normal";
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public string OTReason { get; set; } = null!;
        public bool RequiresGM { get; set; }
        public string RequestStatus { get; set; } = "Pending";
        public bool EmployeeConfirmed { get; set; }

        // Timeline 4 cấp duyệt
        public List<OTApprovalStep> ApprovalSteps { get; set; } = new();

        // Danh sách nhân viên OT
        public List<OTEmployeeModel> Employees { get; set; } = new();
    }
}
