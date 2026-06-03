using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class EmployeeSelectDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
    }
}
