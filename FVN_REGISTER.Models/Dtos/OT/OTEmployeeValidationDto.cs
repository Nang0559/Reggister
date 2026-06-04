using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTEmployeeValidationDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }     // thêm — hiển thị trên UI

        // OTQueryService dùng ViolationType, CurrentUsed, Limit, Requested
        public string ViolationType { get; set; } = string.Empty;  // "Daily"|"Weekly"|"Yearly"
        public decimal CurrentUsed { get; set; }
        public decimal Limit { get; set; }
        public decimal Requested { get; set; }

        // Computed — dùng trực tiếp trong Razor không cần convert
        public bool IsExceeded => CurrentUsed + Requested > Limit;

        public string Message => IsExceeded
            ? $"Vượt giới hạn {ViolationType}: đã dùng {CurrentUsed}h / {Limit}h, yêu cầu thêm {Requested}h"
            : string.Empty;
    }
}
