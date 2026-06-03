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
        public int Level { get; set; }
        public string? Comment { get; set; }
    }
}
