using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTRow
    {
        public int Id { get; set; }
        public int OTRequestId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? DeptCode { get; set; }

        public DateTime PlannedFrom { get; set; }
        public DateTime PlannedTo { get; set; }
        public decimal PlannedHours { get; set; }

        public DateTime? ActualFrom { get; set; }
        public DateTime? ActualTo { get; set; }
        public decimal? ActualHours { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual F03OTRequest Request { get; set; } = null!;
    }
}
