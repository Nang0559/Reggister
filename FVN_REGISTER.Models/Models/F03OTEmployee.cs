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
        public string? DeptCode { get; set; }
        public string? CvCode { get; set; }                     // 0003 = công nhân
        public decimal? ActualHours { get; set; }               // Giờ thực tế (cập nhật sau)
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual F03OTRequest OTRequest { get; set; } = null!;
    }
}
