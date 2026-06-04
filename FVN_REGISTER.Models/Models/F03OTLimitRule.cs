using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
   

    public partial class F03OTLimitRule
    {
        public int Id { get; set; }

        // "Daily" | "Weekly" | "Yearly" | "Special"
        // Khớp với OTLimitType constants
        public string LimitType { get; set; } = null!;

        // Giá trị giới hạn: 4 (daily), 40 (weekly), 200 (yearly), 900 (special)
        public decimal LimitValue { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
