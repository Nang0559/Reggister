
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Maps;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.API.Services.Leaves
{
  

    public class LeaveService : BaseService<LeaveService>, ILeaveService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly ILeaveNotificationService _leaveNotification; // Đầu mối duy nhất lo thông báo
        private readonly ILeaveValidator _validator;
        private readonly IServiceScopeFactory _scopeFactory;

        public LeaveService(
            FVNWEBAPPContext db,
            ILeaveNotificationService leaveNotification,
            ILeaveValidator validator,
            IServiceScopeFactory scopeFactory,
            ILogger<LeaveService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _leaveNotification = leaveNotification;
            _validator = validator;
            _scopeFactory = scopeFactory;
        }

        // ================= DÀNH CHO NHÂN VIÊN =================

        public async Task<ServiceResult> CreateLeaveAsync(
            CreateLeaveRequestModel model,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE] Create start: {User}", user.UserName);

                var session = new UserSessionDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    EmployeeCode = user.EmployeeCode,
                    FullName = user.FullName,
                    Email = user.Email,
                    PermissionCode = user.Permission,
                    DeptCode = user.DeptCode,
                    CVCode = user.CvCode,
                    LevelApprove = user.LevelApprove,
                    IsAdmin = user.IsAdmin()
                };

                var valResult = await _validator.ValidateAsync(model, session, ct);
                if (!valResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[LEAVE] Validation failed: {User}", user.UserName);
                    return valResult;
                }

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var newLeave = new F03leaveDay
                {
                    EmployeeCode = user.EmployeeCode,
                    WorkYear = model.WorkYear,
                    RegisterDate = DateTime.Now,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    TotalDay = model.TotalDay,
                    LeaveReason = model.LeaveReason ?? "",
                    RequestStatus = LeaveStatus.Pending,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = user.UserId,
                    Level1ApproveEmail = model.Level1ApproveEmail,
                    Level1ApproveCode = model.Level1ApproveCode,
                    Level1ApproveName = model.Level1ApproveName,
                    Level2ApproveEmail = model.Level2ApproveEmail,
                    Level2ApproveCode = model.Level2ApproveCode,
                    Level2ApproveName = model.Level2ApproveName,
                    Level3ApproveEmail = model.Level3ApproveEmail,
                    Level3ApproveCode = model.Level3ApproveCode,
                    Level3ApproveName = model.Level3ApproveName,
                };

                _db.F03leaveDays.Add(newLeave);
                await _db.SaveChangesAsync(ct);

                var details = model.Details.Select(d => new F03leaveDayDetail
                {
                    LeaveDaysId = newLeave.Id,
                    LeaveDate = DateOnly.FromDateTime(d.LeaveDate),
                    LeaveTypeCode = d.LeaveTypeCode,
                    LeaveTypeName = d.LeaveTypeName,
                    IsHalfDay = d.IsHalfDay,
                    HalfDayOption = d.HalfDayOption,
                    DayValue = (decimal)d.DayValue,
                    TinhPhep = d.TinhPhep == 1,
                    CreatedAt = DateTime.Now
                });

                _db.F03leaveDayDetails.AddRange(details);
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                Logger.LogInfoIf(Debug, "[LEAVE] Created: {User}", user.UserName);

                // ✅ Lấy dữ liệu cần thiết ra biến cục bộ trước khi vào luồng ngầm
                var capturedLeaveId = newLeave.Id;
                var capturedApproverEmail = newLeave.Level1ApproveEmail;
                var capturedEmpName = user.FullName ?? user.EmployeeCode ?? "";

                // ✅ Fire-and-forget xử lý background
                _ = Task.Run(async () =>
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var leaveNotifySvc = scope.ServiceProvider.GetRequiredService<ILeaveNotificationService>();

                    try
                    {
                        if (string.IsNullOrEmpty(capturedApproverEmail)) return;

                        // Gọi trọn gói thông báo đơn mới đến Người duyệt cấp 1
                        await leaveNotifySvc.NotifyNewLeaveRequestAsync(
                            approverEmail: capturedApproverEmail,
                            leaveId: capturedLeaveId,
                            employeeName: capturedEmpName,
                            cancellationToken: CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "[LEAVE] Background notification failed | LeaveId={LeaveId}", capturedLeaveId);
                    }
                });

                return ServiceResult.Ok("Đã gửi đơn nghỉ thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE] Create error: {Message} | Inner: {Inner}",
                    ex.Message, ex.InnerException?.Message);
                return ServiceResult.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ================= HỦY ĐƠN =================

        public async Task<ServiceResult> CancelAsync(
            int leaveId,
            string reason,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                var leave = await _db.F03leaveDays
                    .FirstOrDefaultAsync(x => x.Id == leaveId, ct);

                if (leave == null)
                    return ServiceResult.Fail("Không tìm thấy đơn.");

                bool isOwner = leave.EmployeeCode == user.EmployeeCode;
                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

                if (!isOwner && !isAdmin)
                    return ServiceResult.Fail("Không có quyền hủy đơn này.");

                if (LeaveStatusHelper.IsApproved(leave.RequestStatus) && !isAdmin)
                    return ServiceResult.Fail("Đơn đã duyệt không thể hủy.");

                leave.IsActive = false;
                leave.RequestStatus = LeaveStatus.Cancel;
                leave.Level1Comment = $"Cancel by {(isOwner ? "User" : "Admin")}: {reason}";
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Đã hủy đơn thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE] Cancel ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi hủy đơn.");
            }
        }

        public async Task<ServiceResult> CancelDetailAsync(
            int detailId,
            string reason,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                var detail = await _db.F03leaveDayDetails
                    .Include(x => x.LeaveDays)
                    .FirstOrDefaultAsync(x => x.Id == detailId, ct);

                if (detail == null)
                    return ServiceResult.Fail("Không tìm thấy ngày nghỉ.");

                var leave = detail.LeaveDays;
                bool isOwner = leave.EmployeeCode == user.EmployeeCode;
                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

                if (!isOwner && !isAdmin)
                    return ServiceResult.Fail("Không có quyền hủy.");

                if (LeaveStatusHelper.IsApproved(leave.RequestStatus) && !isAdmin)
                    return ServiceResult.Fail("Đơn đã duyệt không thể hủy.");

                _db.F03leaveDayDetails.Remove(detail);

                leave.TotalDay -= detail.DayValue;
                if (detail.TinhPhep)
                    leave.TotalLeaveDay = (leave.TotalLeaveDay ?? 0) - detail.DayValue;

                var remainingDetails = await _db.F03leaveDayDetails
                    .CountAsync(x => x.LeaveDaysId == leave.Id, ct);

                if (remainingDetails <= 1)
                {
                    leave.IsActive = false;
                    leave.RequestStatus = LeaveStatus.Cancel;
                    leave.Level1Comment = $"Hủy bởi {user.FullName}: {reason}";
                }

                await _db.SaveChangesAsync(ct);
                return ServiceResult.Ok("Đã hủy ngày nghỉ thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE] CancelDetail ERROR");
                return ServiceResult.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ================= PHÊ DUYỆT / TỪ CHỐI =================

        public async Task<ServiceResult> ApproveAsync(
            List<int> leaveIds, int level, CurrentUser user,
            string comment, CancellationToken ct = default)
            => await ProcessBatchActionAsync(leaveIds, level, user, comment, isApprove: true, ct);

        public async Task<ServiceResult> RejectAsync(
            List<int> leaveIds, int level, CurrentUser user,
            string comment, CancellationToken ct = default)
            => await ProcessBatchActionAsync(leaveIds, level, user, comment, isApprove: false, ct);

        private async Task<ServiceResult> ProcessBatchActionAsync(
            List<int> ids, int level, CurrentUser user,
            string comment, bool isApprove, CancellationToken ct)
        {
            if (ids == null || ids.Count == 0)
                return ServiceResult.Fail("Không có đơn nào được chọn.");

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var leaves = await _db.F03leaveDays
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync(ct);

                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

                var userEmail = await _db.VF03employees
                    .Where(x => x.EmployeeCode == user.EmployeeCode)
                    .Select(x => x.EmailAddress)
                    .FirstOrDefaultAsync(ct) ?? "";

                int success = 0;
                var now = DateTime.Now;
                var processedLeaves = new List<(F03leaveDay Leave, bool IsApprove, int CurrentLevel)>();

                foreach (var leave in leaves)
                {
                    if (LeaveStatusHelper.IsApproved(leave.RequestStatus) ||
                        LeaveStatusHelper.IsRejected(leave.RequestStatus))
                        continue;

                    string approverEmail = level switch
                    {
                        1 => leave.Level1ApproveEmail,
                        2 => leave.Level2ApproveEmail,
                        3 => leave.Level3ApproveEmail,
                        _ => ""
                    } ?? "";

                    if (!isAdmin && !string.Equals(
                        userEmail, approverEmail,
                        StringComparison.OrdinalIgnoreCase))
                        continue;

                    bool canProcess = level switch
                    {
                        1 => leave.Level1IsApprove != true,
                        2 => leave.Level1IsApprove == true && leave.Level2IsApprove != true,
                        3 => leave.Level2IsApprove == true && leave.Level3IsApprove != true,
                        _ => false
                    };

                    if (!canProcess) continue;

                    if (isApprove)
                    {
                        ApplyApproveData(leave, level, user, comment, isAdmin, now);
                        UpdateOverallStatus(leave);
                    }
                    else
                    {
                        ApplyRejectData(leave, level, user, comment, isAdmin, now);
                        leave.RequestStatus = LeaveStatus.Rejected;
                    }

                    processedLeaves.Add((leave, isApprove, level));
                    success++;
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // ✅ Khởi chạy luồng thông báo bất đồng bộ sau khi commit dữ liệu thành công
                if (processedLeaves.Any())
                {
                    _ = Task.Run(async () =>
                    {
                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var db = scope.ServiceProvider.GetRequiredService<FVNWEBAPPContext>();
                        var leaveNotifySvc = scope.ServiceProvider.GetRequiredService<ILeaveNotificationService>();

                        foreach (var (leave, approved, currentLevel) in processedLeaves)
                        {
                            try
                            {
                                // 1. Lấy thông tin tài khoản người gửi đơn (Requester) từ DB nội bộ của Scope chạy ngầm
                                var requesterEmp = await db.F03employees
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.EmployeeCode == leave.EmployeeCode);

                                var requesterUser = await db.F03users
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.EmployeeCode == leave.EmployeeCode);

                                if (requesterEmp != null && requesterUser != null)
                                {
                                    var statusStr = approved ? "Approved" : "Rejected";

                                    // 💥 GỌI CHÍNH XÁC PHƯƠNG THỨC TRONG INTERFACE MỚI CỦA BẠN:
                                    await leaveNotifySvc.NotifyStatusChangedAsync(
                                        requesterUserId: requesterUser.IdUser,
                                        targetEmail: requesterEmp.EmailAddress,
                                        targetName: requesterEmp.EmployeeName,
                                        status: statusStr,
                                        employeeCode: leave.EmployeeCode,
                                        leaveId: leave.Id,
                                        ct: CancellationToken.None);
                                }

                                // 2. Tự động chuyển tiếp thông báo cho Người phê duyệt cấp kế tiếp nếu đơn được đồng ý và chưa hoàn tất luồng
                                if (approved && leave.RequestStatus != LeaveStatus.Approved)
                                {
                                    int nextLevel = currentLevel + 1;
                                    string? nextApproverEmail = nextLevel switch
                                    {
                                        2 => leave.Level2ApproveEmail,
                                        3 => leave.Level3ApproveEmail,
                                        _ => null
                                    };

                                    if (!string.IsNullOrEmpty(nextApproverEmail))
                                    {
                                        string empName = requesterEmp?.EmployeeName ?? leave.EmployeeCode;

                                        // Đẩy thông báo đơn chờ duyệt mới lên cho sếp cấp tiếp theo
                                        await leaveNotifySvc.NotifyNewLeaveRequestAsync(
                                            approverEmail: nextApproverEmail,
                                            leaveId: leave.Id,
                                            employeeName: empName,
                                            cancellationToken: CancellationToken.None);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "[LEAVE] Post-approve background notification failed | LeaveId={LeaveId}", leave.Id);
                            }
                        }
                    });
                }

                return success > 0
                    ? ServiceResult.Ok($"Đã xử lý {success}/{leaves.Count} đơn.")
                    : ServiceResult.Fail("Không có đơn nào đủ điều kiện.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE] Batch ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi xử lý đơn.");
            }
        }

        // ================= QUERIES =================

        public async Task<ServiceResult<LeaveBalanceViewModel>> GetLeaveBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            var phep = await _db.VF03phepTons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);

            var data = new LeaveBalanceViewModel
            {
                EmployeeCode = employeeCode,
                EmployeeName = phep?.EmployeeName ?? "",
                WorkYear = year,
                TotalEntitledLeave = phep?.TongPhep ?? 0,
                UsedLeave = phep?.SoNgayNghiPhep ?? 0,
                RemainingLeave = phep?.PhepTon ?? 0,
                UnpaidLeave = 0,
                SickLeave = 0
            };

            return ServiceResult<LeaveBalanceViewModel>.Ok(data);
        }

        public async Task<ServiceResult<LeaveDaysViewModel>> GetDetailsAsync(
            int leaveId, CancellationToken ct = default)
        {
            var details = await _db.VF03leaveDayDetails
                .AsNoTracking()
                .Where(x => x.LeaveId == leaveId)
                .ToListAsync(ct);

            if (details.Count == 0)
                return ServiceResult<LeaveDaysViewModel>.Fail("Không tìm thấy chi tiết đơn.");

            return ServiceResult<LeaveDaysViewModel>.Ok(LeaveMapper.ToViewModel(details));
        }

        public async Task<ServiceResult<LeaveBalanceDto>> GetPersonalBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            var phep = await _db.VF03phepTons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);

            var pendingDays = await _db.F03leaveDays.AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                         && x.WorkYear == year
                         && x.IsActive == true
                         && x.RequestStatus == LeaveStatus.Pending)
                .SumAsync(x => x.TotalDay, ct);

            var data = new LeaveBalanceDto
            {
                Entitled = phep?.TongPhep ?? 0,
                Used = phep?.SoNgayNghiPhep ?? 0,
                Remaining = phep?.PhepTon ?? 0,
                Pending = pendingDays,
                Year = year
            };

            return ServiceResult<LeaveBalanceDto>.Ok(data);
        }

        public async Task<ServiceResult<List<RecentLeaveRequestDto>>> GetRecentRequestsAsync(
            string employeeCode, int limit, CancellationToken ct = default)
        {
            var list = await _db.F03leaveDays.AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .Select(x => new RecentLeaveRequestDto
                {
                    RequestId = x.Id,
                    FromDate = x.StartDate,
                    ToDate = x.EndDate,
                    Status = x.RequestStatus ?? LeaveStatus.Pending,
                    ApproverName = x.Level1ApproveName
                })
                .ToListAsync(ct);

            return ServiceResult<List<RecentLeaveRequestDto>>.Ok(list);
        }

        // ================= PRIVATE HELPERS =================

        private static void ApplyApproveData(
            F03leaveDay leave, int level, CurrentUser user,
            string comment, bool isAdmin, DateTime now)
        {
            string finalComment = (isAdmin && user.EmployeeCode != GetApproverCode(leave, level))
                ? $"[Admin {user.FullName} duyệt thay]"
                : comment;

            if (level == 1) { leave.Level1IsApprove = true; leave.Level1ApproveTime = now; leave.Level1Comment = finalComment; }
            else if (level == 2) { leave.Level2IsApprove = true; leave.Level2ApproveTime = now; leave.Level2Comment = finalComment; }
            else if (level == 3) { leave.Level3IsApprove = true; leave.Level3ApproveTime = now; leave.Level3Comment = finalComment; }
        }

        private static void ApplyRejectData(
            F03leaveDay leave, int level, CurrentUser user,
            string comment, bool isAdmin, DateTime now)
        {
            string finalComment = isAdmin
                ? $"[Admin {user.FullName} Reject]: {comment}"
                : comment;

            if (level == 1) { leave.Level1IsApprove = false; leave.Level1ApproveTime = now; leave.Level1Comment = finalComment; }
            else if (level == 2) { leave.Level2IsApprove = false; leave.Level2ApproveTime = now; leave.Level2Comment = finalComment; }
            else if (level == 3) { leave.Level3IsApprove = false; leave.Level3ApproveTime = now; leave.Level3Comment = finalComment; }
        }

        private static string? GetApproverCode(F03leaveDay leave, int level) => level switch
        {
            1 => leave.Level1ApproveCode,
            2 => leave.Level2ApproveCode,
            3 => leave.Level3ApproveCode,
            _ => ""
        };

        private static void UpdateOverallStatus(F03leaveDay leave)
        {
            if (!string.IsNullOrEmpty(leave.Level3ApproveEmail) && leave.Level3IsApprove == true)
            { leave.RequestStatus = LeaveStatus.Approved; return; }

            if (leave.Level2IsApprove == true)
            {
                leave.RequestStatus = string.IsNullOrEmpty(leave.Level3ApproveEmail)
                    ? LeaveStatus.Approved
                    : LeaveStatus.ApprovedLv2;
                return;
            }

            if (leave.Level1IsApprove == true)
            {
                leave.RequestStatus = (string.IsNullOrEmpty(leave.Level2ApproveEmail) &&
                                       string.IsNullOrEmpty(leave.Level3ApproveEmail))
                    ? LeaveStatus.Approved
                    : LeaveStatus.ApprovedLv1;
                return;
            }

            leave.RequestStatus = LeaveStatus.Pending;
        }
    }
}
