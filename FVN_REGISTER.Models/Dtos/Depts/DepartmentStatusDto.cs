using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Depts
{
    public class DepartmentStatusDto
    {
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public int TotalEmployees { get; set; }
        public int PresentCount { get; set; }
        public int OnLeaveCount { get; set; }        // Nghỉ có đăng ký phép
        public int AbsentNoReasonCount { get; set; } // Vắng mặt không lý do
        public double AbsenceRate { get; set; }      // % vắng / tổng

        public List<EmployeeStatusRow> Employees { get; set; } = new();
    }
}
