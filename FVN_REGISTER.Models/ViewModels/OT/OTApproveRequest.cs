using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTApproveRequest
    {
        public List<int> Ids { get; set; } = new();
        public int Level { get; set; }              // 3|4|5|6|7
        public string? Comment { get; set; }
    }
}
