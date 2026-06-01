using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Models
{
    public partial class AppNotification
    {
        public int Id { get; set; }

        /// <summary>Người nhận thông báo (UserId từ F03Users)</summary>
        public int UserId { get; set; }

        /// <summary>Mã nhân viên (để query nhanh không cần join)</summary>
        public string? EmployeeCode { get; set; }

        /// <summary>LEAVE_PENDING | LEAVE_APPROVED | LEAVE_REJECTED | SYSTEM</summary>
        public string NotificationType { get; set; } = "SYSTEM";

        /// <summary>Tiêu đề ngắn hiển thị trong danh sách</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Nội dung chi tiết</summary>
        public string? Body { get; set; }

        /// <summary>Link điều hướng khi bấm vào (ví dụ: /leave/details/123)</summary>
        public string? ActionUrl { get; set; }

        /// <summary>ID của đơn nghỉ liên quan (nullable, dùng để nhóm thông báo)</summary>
        public int? RelatedLeaveId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }
    }
}
