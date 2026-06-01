using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace FVN_REGISTER.API.Hubs
{
    
        [Authorize]
        public class NotificationHub : Hub
        {
            // Mỗi user join group theo UserId để push riêng
            public override async Task OnConnectedAsync()
            {
                var userId = Context.User?.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                }
                await base.OnConnectedAsync();
            }

            public override async Task OnDisconnectedAsync(Exception? exception)
            {
                var userId = Context.User?.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                }
                await base.OnDisconnectedAsync(exception);
            }
        }
    
}
