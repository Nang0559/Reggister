using FVN_REGISTER.Contract.Requests.Approvals;
using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Leaves
{
    public class LeaveRequestUpsertDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string LeaveTypeCode { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public List<LeaveRequestDetailUpsertDto> Details { get; set; } = new();

        public List<ApprovalSelectionDto> ApprovalSelections { get; set; } = new();
    }
}
