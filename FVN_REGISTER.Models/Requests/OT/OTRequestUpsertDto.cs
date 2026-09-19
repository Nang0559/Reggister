using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.Approvals;
using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.OT
{
    public class OTRequestUpsertDto
    {
        public int Id { get; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public DateTime OTDate { get; set; } = DateTime.Today;

        [Required, StringLength(10)]
        public string OTTypeCode { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ScopeType { get; set; } = "SELECTED";
        public string OTReasonSummary { get; set; } = string.Empty;

        public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();
        public List<ApprovalSelectionDto> ApprovalSelections { get; set; } = new();
        public List<OTEmployeeDto> Employees { get; set; } = new();
    }
}
