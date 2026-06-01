using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class NotificationClientService : INotificationClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<NotificationClientService> _logger;

        public int UnreadCount { get; private set; }
        public List<NotificationDto> Notifications { get; private set; } = new();

        public event Action? OnChanged;

        public NotificationClientService(
            IHttpClientWithAuth http,
            ILogger<NotificationClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                var result = await _http.GetAsync<object>("api/notification/unread-count", ct);
                if (result.IsSuccess && result.Data is System.Text.Json.JsonElement el)
                {
                    if (el.TryGetProperty("count", out var countEl))
                        UnreadCount = countEl.GetInt32();
                }
                OnChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NOTIFY-CLIENT] InitializeAsync failed");
            }
        }

        public async Task LoadListAsync(CancellationToken ct = default)
        {
            try
            {
                var result = await _http.GetAsync<List<NotificationDto>>("api/notification", ct);
                if (result.IsSuccess && result.Data != null)
                {
                    Notifications = result.Data;
                    UnreadCount = Notifications.Count(x => !x.IsRead);
                    OnChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NOTIFY-CLIENT] LoadListAsync failed");
            }
        }

        public async Task MarkReadAsync(int id, CancellationToken ct = default)
        {
            var result = await _http.PutAsync<object>($"api/notification/{id}/read", new { }, ct);
            if (result.IsSuccess)
            {
                var item = Notifications.FirstOrDefault(x => x.Id == id);
                if (item != null) item.IsRead = true;
                UnreadCount = Math.Max(0, UnreadCount - 1);
                OnChanged?.Invoke();
            }
        }

        public async Task MarkAllReadAsync(CancellationToken ct = default)
        {
            var result = await _http.PutAsync<object>("api/notification/read-all", new { }, ct);
            if (result.IsSuccess)
            {
                Notifications.ForEach(x => x.IsRead = true);
                UnreadCount = 0;
                OnChanged?.Invoke();
            }
        }

        /// <summary>Gọi từ SignalR handler khi nhận push</summary>
        public void HandlePush(NotificationPushDto push)
        {
            UnreadCount = push.UnreadCount;

            if (push.Latest != null && !Notifications.Any(x => x.Id == push.Latest.Id))
                Notifications.Insert(0, push.Latest);

            OnChanged?.Invoke();
        }
    }
}
