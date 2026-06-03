using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTRequestDto
    {
        public int Id { get; set; }
        public DateOnly OTDate { get; set; }
        public string OTType { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string? DeptName { get; set; }
        public string? Reason { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int EmployeeCount { get; set; }
        public List<OTEmployeeDto> Employees { get; set; } = new();

        // Approver display
        public string? Level1ApproveName { get; set; }
        public bool? Level1IsApprove { get; set; }
        public string? Level2ApproveName { get; set; }
        public bool? Level2IsApprove { get; set; }
        public string? Level3ApproveName { get; set; }
        public bool? Level3IsApprove { get; set; }

        public string StatusColor => RequestStatus switch
        {
            "Approved" => "success",
            "Rejected" => "error",
            "ApprovedLv1" => "info",
            "ApprovedLv2" => "info",
            _ => "warning"
        };
    }
}
