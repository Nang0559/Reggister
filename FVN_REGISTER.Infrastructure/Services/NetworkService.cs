

using FVN_REGISTER.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;

namespace FVN_REGISTER.Infrastructure.Utils
{
    public class NetworkService(IHttpContextAccessor httpContextAccessor) : INetworkService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string GetIp()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            // Kiểm tra header X-Forwarded-For nếu chạy sau Load Balancer/Proxy
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim(); // Lấy IP đầu tiên trong chuỗi
            }

            // Nếu không có proxy, lấy IP trực tiếp từ Connection
            return context.Connection.RemoteIpAddress?.ToString() ?? "";
        }
    }
}
