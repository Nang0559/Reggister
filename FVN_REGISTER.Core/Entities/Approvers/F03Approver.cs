using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// FVN_REGISTER.Infrastructure/Models/F03Approver.cs
namespace FVN_REGISTER.Core.Entities.Approvers
{
    /// <summary>
    /// Bảng master: danh sách approver theo phòng ban + cấp duyệt.
    /// Sync từ F03Employee (IsApprove=1) + F03CV.DefaultApprovalLevel.
    ///
    /// Một phòng có thể có nhiều approver cùng Level
    /// (VD: 2 Sub-Leader khác ca) — phân biệt bằng ApproverCode.
    ///
    /// GM/Director duyệt nhiều phòng → ApproveForDeptCode = "ALL".
    /// </summary>
    [Table("F03Approvers")]
    public partial class F03Approver : BaseAuditEntity
    {
        public int? UserId { get; set; }

        [Required]
        public RequestModule RequestType { get; set; }   // đổi từ string, bỏ StringLength(20)

        // ── Thông tin người duyệt ─────────────────────
        [Required, StringLength(50)]
        public string ApproverCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PositionCode { get; set; }

        [StringLength(100)]
        public string ApproverName { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApproverEmail { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string ApproverDeptCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApproverDeptName { get; set; } = string.Empty;

        // ── Phạm vi được phép duyệt ───────────────────
        [Required, StringLength(20)]
        public string ApproveForDeptCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApproveForDeptName { get; set; } = string.Empty;

        // ── Cấp duyệt ────────────────────────────────
        public int Level { get; set; }

        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual F03User? User { get; set; }
    }
}
