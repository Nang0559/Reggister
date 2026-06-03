using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public TimeOnly PlannedFrom { get; set; }
        public TimeOnly PlannedTo { get; set; }
        public decimal PlannedHours { get; set; }
        public TimeOnly? ActualFrom { get; set; }
        public TimeOnly? ActualTo { get; set; }
        public decimal? ActualHours { get; set; }
        public string? OTReason { get; set; }
    }
}
