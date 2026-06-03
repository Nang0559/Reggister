using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class F03OTRequest
    {
        public int Id { get; set; }
        public DateOnly OTDate { get; set; }
        public string OTType { get; set; } = "Normal"; // Normal, Weekend, Holiday
        public string DeptCode { get; set; } = null!;
        public string? Reason { get; set; }
        public string RequestStatus { get; set; } = "Pending";
        public decimal TotalHours { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }

        public string? Level1ApproveCode { get; set; }
        public string? Level1ApproveName { get; set; }
        public string? Level1ApproveEmail { get; set; }
        public bool? Level1IsApprove { get; set; }
        public DateTime? Level1ApproveTime { get; set; }
        public string? Level1Comment { get; set; }

        public string? Level2ApproveCode { get; set; }
        public string? Level2ApproveName { get; set; }
        public string? Level2ApproveEmail { get; set; }
        public bool? Level2IsApprove { get; set; }
        public DateTime? Level2ApproveTime { get; set; }
        public string? Level2Comment { get; set; }

        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }
        public string? Level3ApproveEmail { get; set; }
        public bool? Level3IsApprove { get; set; }
        public DateTime? Level3ApproveTime { get; set; }
        public string? Level3Comment { get; set; }

        public virtual ICollection<F03OTEmployee> Employees { get; set; } = new List<F03OTEmployee>();
    }
}
