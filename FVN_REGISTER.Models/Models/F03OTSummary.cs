using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTSummary
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public int WorkYear { get; set; }
        public int WorkMonth { get; set; }              // 1–12
        public decimal TotalHoursMonth { get; set; }   // Tổng giờ trong tháng
        public decimal TotalHoursYear { get; set; }    // Tổng giờ trong năm
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
