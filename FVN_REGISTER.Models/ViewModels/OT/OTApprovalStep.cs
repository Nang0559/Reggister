using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTApprovalStep
    {
        public int Level { get; set; }

        // "Sub-leader / Leader", "Ast. Chief / Chief", "A.MG / MG", "GM"
        public string LevelName { get; set; } = string.Empty;  // thêm — fix lỗi compiler

        public string RoleName { get; set; } = string.Empty;   // giữ — tên role cụ thể hơn

        public string? ApproverCode { get; set; }               // thêm — cần để match với EmployeeCode
        public string? ApproverName { get; set; }
        public string? ApproverEmail { get; set; }

        public bool? IsApproved { get; set; }
        public DateTime? ApproveTime { get; set; }
        public string? Comment { get; set; }

        public bool IsRequired { get; set; }   // false nếu bỏ qua vì không phải công nhân
        public bool IsSkipped { get; set; }

        // Computed — giữ nguyên, rất tốt
        public string StatusText => IsSkipped ? "Bỏ qua" :
                                    IsApproved == null ? "Chờ duyệt" :
                                    IsApproved == true ? "Đã duyệt" : "Từ chối";

        public string StatusColor => IsSkipped ? "#9E9E9E" :
                                     IsApproved == null ? "#FF9800" :
                                     IsApproved == true ? "#4CAF50" : "#F44336";
    }
}
