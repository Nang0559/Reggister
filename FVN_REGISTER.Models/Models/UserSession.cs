
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Contract.Models
{
    public partial class UserSession
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // "Web" | "Mobile"
        public string DeviceType { get; set; } = null!;

        // Unique fingerprint: Browser UA hash cho Web, DeviceId cho Mobile
        public string DeviceId { get; set; } = null!;

        public string? DeviceName { get; set; }

        public string? JwtToken { get; set; }

        // SignalR connectionId hiện tại (null khi offline)
        public string? SignalRConnectionId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastSeenAt { get; set; }

        public DateTime? RevokedAt { get; set; }
        // 🌟 THÊM DÒNG NÀY VÀO ĐỂ SỬA LỖI ĐỎ:
        public virtual F03user User { get; set; } = null!;
    }
}
