using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTPendingGroup
    {
        public int ApproverLevel { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<OTRequestViewModel> Requests { get; set; } = new();
    }
}
