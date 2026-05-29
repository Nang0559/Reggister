using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class ApprovalCounts
    {
        public int Level1PendingCount { get; set; }
        public int Level2PendingCount { get; set; }
        public int Level3PendingCount { get; set; }
        public List<LeaveDaysViewModel> Requests { get; set; } = new List<LeaveDaysViewModel>(); // Initialize the list
    }
}
