using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security
{
    [Table("F03UserSessions")]
    public partial class F03UserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(10)]
        public string DeviceType { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Lưu HASH (SHA-256) của Refresh Token, KHÔNG lưu raw token.
        /// Tên cột giữ nguyên JwtToken để tránh migration đổi tên.
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string? JwtToken { get; set; }

        public DateTime? ExpiresAt { get; set; }   // ★ MỚI — bắt buộc để check hết hạn

        [StringLength(100)]
        public string? SignalRConnectionId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSeenAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool RememberMe { get; set; }

        [ForeignKey("UserId")]
        public virtual F03User User { get; set; } = null!;
    }
}
