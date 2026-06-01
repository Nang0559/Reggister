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
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool? IsRead { get; set; } // BIT DEFAULT 0
        public DateTime? CreatedAt { get; set; } // DATETIME DEFAULT GETDATE()
        public DateTime? ReadAt { get; set; } // DATETIME NULL
        public int? RefId { get; set; } // INT NULL

        // Navigation property trỏ tới bảng User
        public virtual F03user User { get; set; } = null!;
    }
}
