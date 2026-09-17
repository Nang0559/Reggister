using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FVN_REGISTER.Application.Services.Statics
{
    public class DashboardService : BaseApplicationService<DashboardService>, IDashboardService
    {
        private readonly ILeaveQueryService _leaveQuery;
        private readonly IOTQueryService _otQuery;
        private readonly IStatisticsService _statistics;
        private readonly IApprovalInboxService _approvalInbox;

        public DashboardService(
            ILeaveQueryService leaveQuery,
            IOTQueryService otQuery,
            IStatisticsService statistics,
            IApprovalInboxService approvalInbox,
            ILogger<DashboardService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _leaveQuery = leaveQuery;
            _otQuery = otQuery;
            _statistics = statistics;
            _approvalInbox = approvalInbox;
        }

        public async Task<ServiceResult<DashboardDto>> GetDashboardAsync(
     UserIdentityDto user, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(user.EmployeeCode))
            {
                Logger.LogWarnIf(Debug, "[DASHBOARD] User không có EmployeeCode: UserId={UserId}", user.UserId);
                return ServiceResult<DashboardDto>.Fail("Tài khoản chưa liên kết với hồ sơ nhân viên.");
            }

            var timer = Stopwatch.StartNew();
            try
            {
                bool isManager = user.Permission.IsApprover();

                var data = new DashboardDto
                {
                    ShowManagerView = isManager
                };

                // ================= 1. PENDING APPROVALS (đa module, không tự query lại) =================
                var pendingResult = await _approvalInbox.GetPendingAsync(user, ct);
                if (pendingResult.IsSuccess && pendingResult.Data != null)
                {
                    data.PendingApprovals = pendingResult.Data;
                }
                else
                {
                    Logger.LogWarnIf(Debug,
                        "[DASHBOARD] Không lấy được PendingApprovals: {Msg}", pendingResult.Message);
                }

                // ================= 2. WIDGETS — mỗi module tự đóng góp widget CÁ NHÂN của nó =================
                var leaveWidgetsTask = _leaveQuery.GetMyWidgetsAsync(user.EmployeeCode, ct);
                var otWidgetsTask = _otQuery.GetMyWidgetsAsync(user.EmployeeCode, ct);   // cần xác nhận tên tương tự ở OT

                await Task.WhenAll(leaveWidgetsTask, otWidgetsTask);

                data.Widgets.AddRange(await leaveWidgetsTask);
                data.Widgets.AddRange(await otWidgetsTask);

                // ================= 2b. WIDGET PHÒNG BAN/CÔNG TY — chỉ khi có quyền quản lý =================
                if (isManager)
                {
                    var deptWidgets = !string.IsNullOrEmpty(user.DeptCode)
                        ? await _statistics.GetDeptDashboardWidgetsAsync(user.DeptCode, ct)
                        : await _statistics.GetCompanyDashboardWidgetsAsync(ct);

                    data.Widgets.AddRange(deptWidgets);
                }

                // ================= 3. LEAVE SECTION (cá nhân) =================
                var leaveBalanceTask = _leaveQuery.GetSimpleBalanceAsync(user.EmployeeCode, DateTime.Now.Year, ct);
                var leaveHistoryTask = _leaveQuery.GetRecentSummaryAsync(user.EmployeeCode, 5, ct);

                await Task.WhenAll(leaveBalanceTask, leaveHistoryTask);

                data.Leave = new LeaveDashboardSectionDto
                {
                    Balance = await leaveBalanceTask,
                    RecentRequests = await leaveHistoryTask
                };

                // ================= 4. OT SECTION (cá nhân) =================
                var otBalanceTask = _otQuery.GetSimpleBalanceAsync(user.EmployeeCode, DateTime.Now.Year, ct);
                var otHistoryTask = _otQuery.GetRecentSummaryAsync(user.EmployeeCode, 5, ct);

                await Task.WhenAll(otBalanceTask, otHistoryTask);

                data.Overtime = new OTDashboardSectionDto
                {
                    Balance = await otBalanceTask,
                    RecentRequests = await otHistoryTask
                };

                // ================= 5. MANAGEMENT VIEW (thống kê chi tiết, chỉ khi có quyền) =================
                if (isManager)
                {
                    data.DepartmentStatistics = await _statistics.GetLeaveStatisticsAsync(true, ct);

                    if (!string.IsNullOrEmpty(user.DeptCode))
                    {
                        data.DeptWarning = await _statistics.GetAbsenceWarningAsync(user.DeptCode, ct);
                    }
                }

                timer.Stop();
                Logger.LogInfoIf(Debug,
                    "[DASHBOARD] Loaded in {Ms}ms | User={User} | Manager={IsManager}",
                    timer.ElapsedMilliseconds, user.EmployeeCode, isManager);

                return ServiceResult<DashboardDto>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<DashboardDto>(ex, "Không thể tải dữ liệu Dashboard");
            }
        }
    }
}