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
        public string EmployeeCode { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? DeptCode { get; set; }

        // Có thể khác giờ so với đơn chính (nhân viên theo ca)
        public DateTime? PlannedFrom { get; set; }
        public DateTime? PlannedTo { get; set; }
        public decimal? PlannedHours { get; set; }

        // Tổng giờ hiện tại trong tháng/năm (để hiển thị cảnh báo)
        public decimal CurrentMonthHours { get; set; }
        public decimal CurrentYearHours { get; set; }

        // Sau khi validate
        public bool IsValid { get; set; } = true;
        public string? ValidationMessage { get; set; }
    }
}
