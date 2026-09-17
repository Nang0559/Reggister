using FVN_REGISTER.Contract.Dtos.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Notifications
{
    public interface INotificationClientService
    {
        /// <summary>Số thông báo chưa đọc (cho badge)</summary>
        int UnreadCount { get; }

        /// <summary>Danh sách thông báo hiện tại</summary>
        List<NotificationDto> Notifications { get; }

        /// <summary>Event khi badge/danh sách thay đổi - component subscribe để re-render</summary>
        event Action? OnChanged;

        Task InitializeAsync(CancellationToken ct = default);
        Task LoadListAsync(CancellationToken ct = default);
        Task MarkReadAsync(int id, CancellationToken ct = default);
        Task MarkAllReadAsync(CancellationToken ct = default);

        /// <summary>Gọi khi nhận event SignalR "ReceiveNotification"</summary>
        void HandlePush(NotificationPushDto push);
    }
}
