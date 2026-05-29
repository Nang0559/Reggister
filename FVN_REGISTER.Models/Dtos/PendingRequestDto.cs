using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos
{
    public class PendingRequestDto
    {
        public int RequestId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty; // Nghỉ phép, Nghỉ ốm, Việc riêng...
        public DateTime StartDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}
