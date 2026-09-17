
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.HR;


    [Table("F03Positions")]
    public partial class F03Position : BaseAuditEntity
    {
        [Required, StringLength(20)]
        public string PositionCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string PositionName { get; set; } = string.Empty;

        public bool IsApprove { get; set; } = false;
        public bool IsAllowApprove { get; set; } = false;
    public int? DefaultApproveLevel { get; set; }

    // 🔥 Navigation ngược — 1 Position có nhiều Employee
    public virtual ICollection<F03Employee> Employees { get; set; } = new List<F03Employee>();
    }



