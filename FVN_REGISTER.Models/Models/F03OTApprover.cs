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

        public string DeptCode { get; set; } = null!;   // "ALL" = áp dụng tất cả phòng ban

        public string? DeptName { get; set; }

        // Cấp duyệt theo flow: 3 | 5 | 6 | 7
        public int ApproveLevel { get; set; }

        // Tên bước cố định: "Sub-leader / Leader", "Ast. Chief / Chief", "A.MG / MG", "GM"
        public string? LevelName { get; set; }

        // Tên chức danh thực tế từ DB
        public string? RoleName { get; set; }

        public string ApproverCode { get; set; } = null!;

        public string ApproverName { get; set; } = null!;

        public string ApproverEmail { get; set; } = null!;

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
