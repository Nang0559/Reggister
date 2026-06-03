using FVN_REGISTER.Contract.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTRequestViewModel
    {
        public int Id { get; set; }
        public string OTCode { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string? DeptName { get; set; }
        public DateTime OTDate { get; set; }
        public string OTType { get; set; } = string.Empty;
        public string OTTypeText { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal PlannedHours { get; set; }
        public string? OTReason { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }

        public List<OTApprovalStep> ApprovalSteps { get; set; } = new();
        public List<OTEmployeeModel> Employees { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Computed
        public bool CanEdit => OTStatus.ActiveStatuses.Contains(RequestStatus);
        public bool CanCancel => OTStatus.ActiveStatuses.Contains(RequestStatus);
    }
}
