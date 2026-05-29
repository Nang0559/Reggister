

namespace FVN_REGISTER.Contract.Interfaces.Users
{
    public interface IUserLogService
    {
        // Chuyển sang Task để chạy bất đồng bộ
        Task UpdateLastSeenAsync(int userId, string name, string url);
    }
}
