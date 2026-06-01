using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace FVN_REGISTER.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Client tự join group theo UserId khi kết nối.
        /// Group name = "user_{userId}" để push riêng từng người.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst("UserId")?.Value
                           ?? Context.User?.FindFirst("nameid")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userIdClaim}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdClaim = Context.User?.FindFirst("UserId")?.Value
                           ?? Context.User?.FindFirst("nameid")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userIdClaim}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

}
