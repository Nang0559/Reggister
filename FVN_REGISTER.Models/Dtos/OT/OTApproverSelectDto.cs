using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTApproverSelectDto
    {
        public int Id { get; set; }
        public string DeptCode { get; set; } = "";
        public string DeptName { get; set; } = "";
        public int ApproveLevel { get; set; }  // 3,4,5,6,7

        // Role label: "SubLeader" | "Union" | "Chief" | "MG" | "GM"
        public string Role { get; set; } = "";

        public string? ApproveLevelCode { get; set; }
        public string ApproveLevelName { get; set; } = "";
        public string ApproveLevelEmail { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
