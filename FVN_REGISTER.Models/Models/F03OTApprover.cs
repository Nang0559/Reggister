using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTApprover
    {
        public int Id { get; set; }
        public string DeptCode { get; set; } = null!;
        public int ApproveLevel { get; set; }
        public string? RoleName { get; set; }
        public string? ApproveLevelCode { get; set; }
        public string ApproveLevelName { get; set; } = null!;
        public string ApproveLevelEmail { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; } = -1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ModifiedBy { get; set; } = -1;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }
}
