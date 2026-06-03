using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Reports
{
    public class ReportColumnDef
    {
        public string Field { get; set; } = string.Empty;  // key trong Row dict
        public string Header { get; set; } = string.Empty;
        public string DataType { get; set; } = "string";   // "string"|"number"|"date"|"decimal"
        public string? Format { get; set; }                // "dd/MM/yyyy" | "N2" | ...
        public bool IsVisible { get; set; } = true;
        public string? CssClass { get; set; }
    }
}
