using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApproverTreeNodeDto
    {
        public string DeptCode { get; set; } = "";
        public string DeptName { get; set; } = "";
        public List<ApproverTreeLevelGroupDto> Levels { get; set; } = new();
    }
}
