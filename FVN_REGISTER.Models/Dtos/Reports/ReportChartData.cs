using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Reports
{
    public class ReportChartData
    {
        public string ChartType { get; set; } = "bar";     // "bar"|"line"|"pie"|"donut"
        public List<string> Labels { get; set; } = new();
        public List<ReportChartSeries> Series { get; set; } = new();
    }
}
