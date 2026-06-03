using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class VF03OTSummary
    {
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? DeptCode { get; set; }
        public int OTYear { get; set; }
        public int OTMonth { get; set; }
        public DateOnly OTDate { get; set; }
        public string OTType { get; set; } = null!;
        public decimal OTHours { get; set; }
        public string RequestStatus { get; set; } = null!;
    }
}
