using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Reports
{
    public class ReportChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string? Color { get; set; }
    }
}
