using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models;

namespace FVN_REGISTER.API.Services.Users
{
    public class UserLogService : IUserLogService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLogService(FVNWEBAPPContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task UpdateLastSeenAsync(int userId, string name, string url)
        {
            var context = _httpContextAccessor.HttpContext;

            // Lấy IP của máy nhân viên đang gọi API
            string clientIp = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            // Lấy User Agent (Trình duyệt hoặc loại thiết bị di động)
            string userAgent = context?.Request.Headers["User-Agent"].ToString() ?? "API Client";

            var log = new F03userLog // Tên bảng theo DbContext mới của bạn
            {
                UserId = userId,
                ApplicationName = "FVNAP API Service",
                ApplicationVerion = "2.0.0", // Nâng version cho kỷ nguyên API nhé!

                // Thay vì tên máy chủ, ta lưu IP của Client hoặc thông tin thiết bị
                WorkstationName = clientIp,
                WorkstationUser = userAgent.Length > 250 ? userAgent.Substring(0, 250) : userAgent,

                LastSeen = name,
                LastSeenUrl = url,
                CreatedAt = DateTime.Now
            };

            _db.F03userLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
