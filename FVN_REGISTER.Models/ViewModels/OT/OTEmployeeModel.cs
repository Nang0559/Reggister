using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTEmployeeModel
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public TimeOnly PlannedFrom { get; set; } = new(17, 0);
        public TimeOnly PlannedTo { get; set; } = new(20, 0);
        public string? OTReason { get; set; }
        public bool IsAstChiefOrAbove { get; set; } // từ CvCode
        public bool IsOfficeStaff { get; set; }   // NV văn phòng (bỏ qua bước Sub.Leader)
    }
}
