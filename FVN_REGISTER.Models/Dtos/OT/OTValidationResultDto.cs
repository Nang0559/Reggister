using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTValidationResultDto
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Chi tiết kiểm tra từng nhân viên
        public List<OTEmployeeValidationDto> EmployeeResults { get; set; } = new();
    }
}
