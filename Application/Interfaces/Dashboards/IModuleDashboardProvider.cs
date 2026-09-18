using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Interfaces.Dashboards
{
    /// <summary>
    /// Mỗi module nghiệp vụ (Leave/Overtime/Trip) tự implement 1 provider, đăng ký DI dạng
    /// collection (AddScoped&lt;IModuleDashboardProvider, LeaveDashboardProvider&gt;() ...).
    /// DashboardOrchestrator chỉ lặp qua toàn bộ provider, KHÔNG biết và KHÔNG cần biết
    /// có bao nhiêu module hay module nào - thêm module mới = thêm 1 provider mới,
    /// không sửa Orchestrator.
    /// </summary>
    public interface IModuleDashboardProvider
    {
        RequestModule Module { get; }

        /// <summary>
        /// Provider tự quyết định trả về gì cho user này (kể cả trả về Widgets rỗng nếu
        /// user không có quyền xem phần quản lý của module) - vì chỉ provider mới đủ
        /// ngữ cảnh nghiệp vụ của module đó.
        /// </summary>
        Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default);
    }
}
