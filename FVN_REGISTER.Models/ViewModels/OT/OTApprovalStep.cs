using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTApprovalStep
    {
        public int Level { get; set; }              // 3,4,5,6,7
        public string StepName { get; set; } = null!; // "Sub.Leader", "BCH CĐ", ...
        public string? ApproverName { get; set; }
        public string? ApproverEmail { get; set; }
        public bool? IsApproved { get; set; }       // null=chờ, true=duyệt, false=từ chối
        public DateTime? ApproveTime { get; set; }
        public string? Comment { get; set; }
        public bool IsSkipped { get; set; }
        public bool IsRequired { get; set; }        // false = GM khi không cần

        public string StatusColor => IsSkipped ? "#9E9E9E"
            : IsApproved == true ? "#4CAF50"
            : IsApproved == false ? "#F44336"
            : "#FF9800";

        public string StatusIcon => IsSkipped ? "RemoveCircle"
            : IsApproved == true ? "CheckCircle"
            : IsApproved == false ? "Cancel"
            : "HourglassEmpty";

        public string StatusText => IsSkipped ? "Bỏ qua"
            : IsApproved == true ? "Đã duyệt"
            : IsApproved == false ? "Từ chối"
            : IsRequired ? "Chờ duyệt" : "Không cần";
    }
}
