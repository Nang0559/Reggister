using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<OTHoursWarning> Warnings { get; set; } = new();
    }
}
