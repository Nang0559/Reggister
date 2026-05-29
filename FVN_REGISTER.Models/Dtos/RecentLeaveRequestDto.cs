using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos
{
    public class RecentLeaveRequestDto
    {
        public int RequestId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Status { get; set; } = string.Empty; // Approved, Rejected, Processing
        public string? ApproverName { get; set; }          // Người duyệt (để biết đơn đang "kẹt" ở ai)
        public string StatusColor => Status switch         // Helper để hiển thị màu MudChip
        {
            "Approved" => "success",
            "Rejected" => "error",
            _ => "warning"
        };
    }
}
