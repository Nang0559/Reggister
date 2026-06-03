using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTApproverSelectDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DeptCode { get; set; } = null!;
        public string DeptName { get; set; } = null!;

        // Role trong luồng duyệt OT
        // "SubLeader" | "Leader" | "UnionRep" | "AstChief" | "Chief" | "AMG" | "MG" | "GM"
        public string Role { get; set; } = null!;

        // Cấp duyệt trong workflow: 3 | 4 | 5 | 6 | 7
        public int ApproveLevel { get; set; }

        public string DisplayText => $"{Name} ({Role}) - {DeptName}";
    }
}
