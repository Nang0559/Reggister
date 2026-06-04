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

        // Thêm — OTQueryService trả về result.Message, client check field này
        public string Message { get; set; } = string.Empty;

        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public List<OTEmployeeValidationDto> EmployeeResults { get; set; } = new();
    }
}
