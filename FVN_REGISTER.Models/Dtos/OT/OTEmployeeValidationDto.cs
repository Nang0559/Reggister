using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTEmployeeValidationDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public string? Error { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal RemainingMonthly { get; set; }
        public decimal RemainingYearly { get; set; }
    }
}
