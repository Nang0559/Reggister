using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



   
    namespace FVN_REGISTER.Core.Entities.Approvers
    {
        /// <summary>
        /// Bảng tracking: snapshot các bước duyệt thực tế của từng đơn.
        ///
        /// Tạo tự động khi submit đơn dựa trên:
        ///   - ApprovalRoleResolver.GetLeaveSteps() / GetOTSteps()
        ///   - F03Approver (lấy approver theo dept + level)
        ///   - CvCode người đăng ký (tính Required)
        ///
        /// KHÔNG sửa sau khi tạo, trừ cột Approved/ApprovedAt/Comment
        /// và IsOverriddenByAdmin khi admin can thiệp.
        ///
        /// Tính overall status của đơn:
        ///   - Approved = All Required steps có Approved=true
        ///   - Rejected = Bất kỳ Required step nào có Approved=false
        ///   - Pending  = Còn Required step chưa xử lý (Approved=null)
        /// </summary>
        [Table("F03ApprovalSteps")]
        public partial class F03ApprovalStep : BaseAuditEntity
        {
            [Required, StringLength(20)]
            public RequestModule RequestType { get; set; } = RequestModule.Overtime; // LEAVE, OT, TRIP

            [Required]
            public int RequestId { get; set; } // FK tới các bảng đơn tương ứng

            // ── Thông tin bước ───────────────────────────
            public int Level { get; set; }

            [StringLength(50)]
            public string? LevelName { get; set; }

            [Required, StringLength(50)]
            public string RoleName { get; set; } = string.Empty;

            // ── Snapshot approver ────────────────────────
            [StringLength(50)]
            public string? ApproverCode { get; set; }

            [StringLength(100)]
            public string? ApproverName { get; set; }

            [StringLength(100)]
            public string? ApproverEmail { get; set; }

            // ── Cấu hình & Kết quả ───────────────────────
            public bool Required { get; set; } = true;
            public bool? Approved { get; set; }
            public DateTime? ApprovedAt { get; set; }

            [StringLength(500)]
            public string? Comment { get; set; }

            public bool ReminderSent { get; set; } = false;

            // ── Admin override ──────────────────────────
            public bool IsOverriddenByAdmin { get; set; } = false;

            [StringLength(50)]
            public string? OverriddenByCode { get; set; }

            [StringLength(100)]
            public string? OverriddenByName { get; set; }

            public DateTime? OverriddenAt { get; set; }
        }
    }

