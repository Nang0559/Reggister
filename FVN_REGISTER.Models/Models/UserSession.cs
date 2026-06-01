
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Contract.Models
{
    public partial class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DeviceId { get; set; } = null!;
        public string DeviceType { get; set; } = null!;
        public string? DeviceName { get; set; }
        public string RefreshToken { get; set; } = null!;

        [Column("IsActive")]
        public bool IsRevoked { get; set; } // 👈 Trong code gọi IsRevoked (false), EF tự hiểu dưới SQL là IsActive (0)

        public DateTime? CreatedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }

        [Column("ExpiredAt")]
        public DateTime ExpireTime { get; set; } // 👈 Trong code gọi ExpireTime, EF tự hiểu dưới SQL là ExpiredAt

        public virtual F03user User { get; set; } = null!;
    }
}
