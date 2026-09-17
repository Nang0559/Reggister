using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Depts
{
    public class EmployeeStatusRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // PRESENT | ON_LEAVE | ABSENT
        public string StatusDisplay { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;

        // Nếu ON_LEAVE
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public string? LeaveReason { get; set; }

        // Nếu PRESENT
        public string? CheckInText { get; set; }
        public string? CheckOutText { get; set; }
    }
}
