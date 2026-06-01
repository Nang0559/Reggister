using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? ActionUrl { get; set; }
        public int? RelatedLeaveId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Helper cho UI
        public string TimeAgo => GetTimeAgo(CreatedAt);
        public string IconName => NotificationType switch
        {
            "LEAVE_PENDING" => "HourglassEmpty",
            "LEAVE_APPROVED" => "CheckCircle",
            "LEAVE_REJECTED" => "Cancel",
            _ => "Notifications"
        };
        public string Color => NotificationType switch
        {
            "LEAVE_PENDING" => "warning",
            "LEAVE_APPROVED" => "success",
            "LEAVE_REJECTED" => "error",
            _ => "info"
        };

        private static string GetTimeAgo(DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return dt.ToString("dd/MM/yyyy");
        }
    }
}
