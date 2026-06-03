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
        [Required] public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }

        [Required] public TimeOnly PlannedFrom { get; set; }
        [Required] public TimeOnly PlannedTo { get; set; }

        // Lý do OT (khớp với form thực tế: A,B,C,D,E,F,G,H,I)
        public string? OTReason { get; set; }

        // Tính tự động
        public decimal PlannedHours =>
            (decimal)(PlannedTo - PlannedFrom).TotalHours;
    }
}
