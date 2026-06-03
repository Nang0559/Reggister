using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTEmployee
    {
        public int Id { get; set; }
        public int OTRequestId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public TimeOnly PlannedFrom { get; set; }
        public TimeOnly PlannedTo { get; set; }
        public TimeOnly? ActualFrom { get; set; }
        public TimeOnly? ActualTo { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public string? OTReason { get; set; }  // A,B,C,D,E,F,G,H,I
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public virtual F03OTRequest OTRequest { get; set; } = null!;
    }
}
