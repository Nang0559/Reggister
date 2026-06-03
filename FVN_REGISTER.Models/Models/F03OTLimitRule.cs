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
        public string RuleType { get; set; } = null!;           // DAILY | MONTHLY | YEARLY | SPECIAL_YEARLY
        public string? OTType { get; set; }                     // null = áp dụng tất cả
        public decimal MaxHours { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
