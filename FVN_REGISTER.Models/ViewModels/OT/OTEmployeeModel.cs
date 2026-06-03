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
        public string EmployeeCode { get; set; } = "";
        public string? EmployeeName { get; set; }
        public string? DeptCode { get; set; }
        public DateTime PlannedFrom { get; set; }
        public DateTime PlannedTo { get; set; }
        public decimal PlannedHours { get; set; }

        // Giờ thực tế (điền sau khi OT xong)
        public DateTime? ActualFrom { get; set; }
        public DateTime? ActualTo { get; set; }
        public decimal? ActualHours { get; set; }
    }
}
