using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.Statics;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FVN_REGISTER.API.Services.Statics
{
    public class DashboardService : BaseService<DashboardService>, IDashboardService
    {
        private readonly IStatisticsService _statistics;
        private readonly ILeaveService _leave;
        private readonly ILeaveQueryService _query;

        public DashboardService(
            IStatisticsService statistics,
            ILeaveService leave,
            ILeaveQueryService query,
            ILogger<DashboardService> logger,
            IOptionsMonitor<AuthDebugOptions> options
        ) : base(logger, options)
        {
            _statistics = statistics;
            _leave = leave;
            _query = query;
        }

        public async Task<ServiceResult<DashboardViewModel>> GetDashboardAsync(CurrentUser user, CancellationToken ct)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                // TẬN DỤNG CÁC HÀM DTO MỚI - Dữ liệu sẽ load rất nhanh

                // 1. Lấy các thẻ Widget (Gom cả chờ duyệt, vắng mặt, tổng NV vào đây)
                var widgets = await _query.GetDashboardWidgetsAsync(user.EmployeeCode, user.DeptCode, ct);

                // 2. Lấy số dư phép (Bản DTO nhẹ)
                var balance = await _query.GetSimpleBalanceAsync(user.EmployeeCode, DateTime.Now.Year, ct);

                // 3. Lấy lịch sử 5 đơn gần nhất
                var history = await _query.GetRecentHistoryAsync(user.EmployeeCode, 5, ct);

                // 4. Các dữ liệu thống kê nặng hơn (HR)
                var leaveStats = await _statistics.GetLeaveStatisticsAsync(true, ct);

                var data = new DashboardViewModel
                {
                    CurrentUser = user,
                    Widgets = widgets,
                    PersonalBalance = balance,
                    RecentRequests = history,
                    LeaveStatistics = leaveStats
                };

                // Nếu là Manager, lấy thêm cảnh báo vắng mặt của phòng
                if (!string.IsNullOrEmpty(user.DeptCode))
                {
                    data.DeptWarning = await _statistics.GetAbsenceWarningAsync(user.DeptCode, ct);
                }

                timer.Stop();
                Logger.LogInfoIf(Debug, "[{Component}] Dashboard loaded in {ms}ms", ComponentName, timer.ElapsedMilliseconds);

                return ServiceResult<DashboardViewModel>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<DashboardViewModel>(ex, "Không thể tải dữ liệu Dashboard");
            }
        }
    }
}
