using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos
{
    /// <summary>Payload gửi qua SignalR tới client</summary>
    public class NotificationPushDto
    {
        public int UnreadCount { get; set; }
        public NotificationDto? Latest { get; set; }
    }
}
