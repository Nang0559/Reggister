using FVN_REGISTER.API.Services.Notifications.FVN_REGISTER.API.Services.Notifications;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Auths;
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
        private readonly ILeaveNotificationService _notification;
        private readonly ILeaveValidator _validator;
        private readonly INotificationService _notificationService;
        private readonly IServiceScopeFactory _scopeFactory;
        public LeaveService(
            FVNWEBAPPContext db,
            ILeaveNotificationService notification,
            ILeaveValidator validator,
            ILogger<LeaveService> logger,
            INotificationService notificationService,
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _notification = notification;
            _validator = validator;
            _notificationService = notificationService;
            _scopeFactory = scopeFactory;
        }

        // ================= DÀNH CHO NHÂN VIÊN =================

        public async Task<ServiceResult> CreateLeaveAsync(CreateLeaveRequestModel model, CurrentUser user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE] Create start: {User}", user.UserName);

                // 1. Ánh xạ thủ công sang UserSessionDto nếu Validator chưa nâng cấp
                var session = new UserSessionDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    EmployeeCode = user.EmployeeCode,
                    FullName = user.FullName,
                    Email = user.Email,
                    PermissionCode = user.Permission, // Lưu ý mapping Permission sang PermissionCode
                    DeptCode = user.DeptCode,
                    CVCode = user.CvCode,
                    LevelApprove = user.LevelApprove,
                    IsAdmin = user.IsAdmin()
                };

                // 2. Gọi validator với object session vừa tạo
                var valResult = await _validator.ValidateAsync(model, session, ct);

                if (!valResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[LEAVE] Validation failed: {User}", user.UserName);
                    return valResult;
                }

                // --- Giữ nguyên logic Database bên dưới, sử dụng 'user' (CurrentUser) ---
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

                    // ✅ Level 1
                    Level1ApproveEmail = model.Level1ApproveEmail,
                    Level1ApproveCode = model.Level1ApproveCode,
                    Level1ApproveName = model.Level1ApproveName,

                    // ✅ Level 2
                    Level2ApproveEmail = model.Level2ApproveEmail,
                    Level2ApproveCode = model.Level2ApproveCode,
                    Level2ApproveName = model.Level2ApproveName,

                    // ✅ Level 3
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

                // Sau dòng await tx.CommitAsync(ct);
                // Sau dòng await tx.CommitAsync(ct);

                // Capture các giá trị cần thiết TRƯỚC khi vào Task.Run
                // vì object có thể bị dispose sau khi request kết thúc
                var capturedLeaveId = newLeave.Id;
                var capturedEmail = newLeave.Level1ApproveEmail;
                var capturedStartDate = newLeave.StartDate;
                var capturedEndDate = newLeave.EndDate;
                var capturedTotalDay = newLeave.TotalDay;
                var capturedReason = newLeave.LeaveReason;
                var capturedEmpName = user.FullName ?? user.EmployeeCode ?? "";

                _ = Task.Run(async () =>
                {
                    // Tạo scope mới — DbContext riêng, không bị dispose cùng request
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<FVNWEBAPPContext>();
                    var emailSvc = scope.ServiceProvider.GetRequiredService<ILeaveNotificationService>();
                    var notifySvc = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    try
                    {
                        // 1. Queue email — dùng db mới trong scope
                        if (!string.IsNullOrEmpty(capturedEmail))
                        {
                            // Rebuild leave object tối giản để gửi email
                            var leaveForEmail = new F03leaveDay
                            {
                                Id = capturedLeaveId,
                                StartDate = capturedStartDate,
                                EndDate = capturedEndDate,
                                TotalDay = capturedTotalDay,
                                LeaveReason = capturedReason,
                                Level1ApproveEmail = capturedEmail
                            };

                            await emailSvc.SendApprovalRequestAsync(
                                capturedEmail,
                                "Approver",
                                leaveForEmail,
                                capturedEmpName,
                                CancellationToken.None); // Dùng None vì request scope đã kết thúc
                        }

                        // 2. Tạo AppNotification cho approver — tìm UserId từ email
                        if (!string.IsNullOrEmpty(capturedEmail))
                        {
                            var approverUser = await db.F03users
                                .AsNoTracking()
                                .Join(db.F03employees,
                                    u => u.EmployeeCode,
                                    e => e.EmployeeCode,
                                    (u, e) => new { u.IdUser, e.EmailAddress })
                                .FirstOrDefaultAsync(x =>
                                    x.EmailAddress == capturedEmail);

                            if (approverUser != null)
                            {
                                await notifySvc.NotifyPendingLeaveAsync(
                                    approverUserId: approverUser.IdUser,
                                    employeeName: capturedEmpName,
                                    leaveId: capturedLeaveId,
                                    ct: CancellationToken.None);

                                Logger.LogDebugIf(Debug,
                                    "[LEAVE] Notification pushed | ApproverUserId={UserId} | LeaveId={LeaveId}",
                                    approverUser.IdUser, capturedLeaveId);
                            }
                            else
                            {
                                Logger.LogWarning(
                                    "[LEAVE] Approver not found by email={Email} | LeaveId={LeaveId}",
                                    capturedEmail, capturedLeaveId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex,
                            "[LEAVE] Background notification failed | LeaveId={LeaveId}",
                            capturedLeaveId);
                    }
                }); // Không truyền ct vào Task.Run vì request đã kết thúc

                return ServiceResult.Ok("Đã gửi đơn nghỉ thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE] Create error: {Message} | Inner: {Inner}",
                ex.Message,
                ex.InnerException?.Message);
                return ServiceResult.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ServiceResult> CancelAsync(int leaveId, string reason, CurrentUser user, CancellationToken ct = default)
        {
            try
            {
                var leave = await _db.F03leaveDays.FirstOrDefaultAsync(x => x.Id == leaveId, ct);
                if (leave == null) return ServiceResult.Fail("Không tìm thấy đơn.");

                bool isOwner = leave.EmployeeCode == user.EmployeeCode;
                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

                if (!isOwner && !isAdmin) return ServiceResult.Fail("Không có quyền hủy đơn này.");
                if (LeaveStatusHelper.IsApproved(leave.RequestStatus) && !isAdmin) return ServiceResult.Fail("Đơn đã duyệt không thể hủy.");

                leave.IsActive = false;
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
        // LeaveService.cs
        public async Task<ServiceResult> CancelDetailAsync(
            int detailId,
            string reason,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Lấy detail
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

                // 2. Xóa detail
                _db.F03leaveDayDetails.Remove(detail);

                // 3. Cập nhật lại TotalDay của đơn cha
                leave.TotalDay -= detail.DayValue;
                if (detail.TinhPhep)
                    leave.TotalLeaveDay = (leave.TotalLeaveDay ?? 0) - detail.DayValue;

                // 4. Nếu không còn detail nào → hủy luôn cả đơn
                var remainingDetails = await _db.F03leaveDayDetails
                    .CountAsync(x => x.LeaveDaysId == leave.Id, ct);

                if (remainingDetails <= 1) // <= 1 vì chưa SaveChanges
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
        // ================= DÀNH CHO QUẢN LÝ =================

        public async Task<ServiceResult> ApproveAsync(List<int> leaveIds, int level, CurrentUser user, string comment, CancellationToken ct = default)
            => await ProcessBatchActionAsync(leaveIds, level, user, comment, isApprove: true, ct);

        public async Task<ServiceResult> RejectAsync(List<int> leaveIds, int level, CurrentUser user, string comment, CancellationToken ct = default)
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

                // ✅ Lấy email của user hiện tại để so sánh
                var userEmail = await _db.VF03employees
                    .Where(x => x.EmployeeCode == user.EmployeeCode)
                    .Select(x => x.EmailAddress)
                    .FirstOrDefaultAsync(ct) ?? "";

                int success = 0;
                var now = DateTime.Now;

                foreach (var leave in leaves)
                {
                    // Bỏ qua đơn đã xong
                    if (LeaveStatusHelper.IsApproved(leave.RequestStatus) ||
                        LeaveStatusHelper.IsRejected(leave.RequestStatus))
                        continue;

                    // ✅ So sánh theo EMAIL thay vì EmployeeCode
                    string approverEmail = level switch
                    {
                        1 => leave.Level1ApproveEmail,
                        2 => leave.Level2ApproveEmail,
                        3 => leave.Level3ApproveEmail,
                        _ => ""
                    } ?? "";

                    // ✅ Check quyền theo email
                    if (!isAdmin && !string.Equals(
                        userEmail, approverEmail,
                        StringComparison.OrdinalIgnoreCase))
                        continue;

                    // ✅ Check điều kiện theo cấp
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
                    success++;
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

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

        // ================= QUERIES (Khớp với LeaveBalanceViewModel mới) =================

        public async Task<ServiceResult<LeaveBalanceViewModel>> GetLeaveBalanceAsync(string employeeCode, int year, CancellationToken ct = default)
        {
            var phep = await _db.VF03phepTons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);

            // Khớp hoàn toàn với các thuộc tính decimal của LeaveBalanceViewModel
            var data = new LeaveBalanceViewModel
            {
                EmployeeCode = employeeCode,
                EmployeeName = phep?.EmployeeName ?? "",
                WorkYear = year,
                TotalEntitledLeave = phep?.TongPhep ?? 0,
                UsedLeave = phep?.SoNgayNghiPhep ?? 0,
                RemainingLeave = phep?.PhepTon ?? 0,
                UnpaidLeave = 0, // Cập nhật logic nếu có cột này trong DB
                SickLeave = 0    // Cập nhật logic nếu có cột này trong DB
            };

            return ServiceResult<LeaveBalanceViewModel>.Ok(data);
        }

        public async Task<ServiceResult<LeaveDaysViewModel>> GetDetailsAsync(int leaveId, CancellationToken ct = default)
        {
            var details = await _db.VF03leaveDayDetails.AsNoTracking().Where(x => x.LeaveId == leaveId).ToListAsync(ct);
            if (details.Count == 0) return ServiceResult<LeaveDaysViewModel>.Fail("Không tìm thấy chi tiết đơn.");

            return ServiceResult<LeaveDaysViewModel>.Ok(LeaveMapper.ToViewModel(details));
        }

        // Tương tự cho Dashboard
        public async Task<ServiceResult<LeaveBalanceDto>> GetPersonalBalanceAsync(string employeeCode, int year, CancellationToken ct = default)
        {
            // 1. Lấy thông tin phép tổng quát từ View
            var phep = await _db.VF03phepTons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);

            // 2. Tính số ngày đang chờ duyệt (Pending) từ bảng đơn nghỉ
            // Chỉ lấy những đơn Active và có trạng thái là Pending
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
                Pending = pendingDays, // Gán giá trị vừa tính được
                Year = year
            };

            return ServiceResult<LeaveBalanceDto>.Ok(data);
        }

        public async Task<ServiceResult<List<RecentLeaveRequestDto>>> GetRecentRequestsAsync(string employeeCode, int limit, CancellationToken ct = default)
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
                }).ToListAsync(ct);

            return ServiceResult<List<RecentLeaveRequestDto>>.Ok(list);
        }

        // ================= HELPERS (Private) =================

        private static void ApplyApproveData(F03leaveDay leave, int level, CurrentUser user, string comment, bool isAdmin, DateTime now)
        {
            string finalComment = (isAdmin && user.EmployeeCode != GetApproverCode(leave, level)) ? $"[Admin {user.FullName} duyệt thay]" : comment;
            if (level == 1) { leave.Level1IsApprove = true; leave.Level1ApproveTime = now; leave.Level1Comment = finalComment; }
            else if (level == 2) { leave.Level2IsApprove = true; leave.Level2ApproveTime = now; leave.Level2Comment = finalComment; }
            else if (level == 3) { leave.Level3IsApprove = true; leave.Level3ApproveTime = now; leave.Level3Comment = finalComment; }
        }

        private static void ApplyRejectData(F03leaveDay leave, int level, CurrentUser user, string comment, bool isAdmin, DateTime now)
        {
            string finalComment = isAdmin ? $"[Admin {user.FullName} Reject]: {comment}" : comment;
            if (level == 1) { leave.Level1IsApprove = false; leave.Level1Comment = finalComment; leave.Level1ApproveTime = now; }
            else if (level == 2) { leave.Level2IsApprove = false; leave.Level2Comment = finalComment; leave.Level2ApproveTime = now; }
            else if (level == 3) { leave.Level3IsApprove = false; leave.Level3Comment = finalComment; leave.Level3ApproveTime = now; }
        }

        private static string? GetApproverCode(F03leaveDay leave, int level) => level switch { 1 => leave.Level1ApproveCode, 2 => leave.Level2ApproveCode, 3 => leave.Level3ApproveCode, _ => "" };

        private static void UpdateOverallStatus(F03leaveDay leave)
        {
            if (!string.IsNullOrEmpty(leave.Level3ApproveEmail) && leave.Level3IsApprove == true) { leave.RequestStatus = LeaveStatus.Approved; return; }
            if (leave.Level2IsApprove == true) { leave.RequestStatus = string.IsNullOrEmpty(leave.Level3ApproveEmail) ? LeaveStatus.Approved : LeaveStatus.ApprovedLv2; return; }
            if (leave.Level1IsApprove == true) { leave.RequestStatus = (string.IsNullOrEmpty(leave.Level2ApproveEmail) && string.IsNullOrEmpty(leave.Level3ApproveEmail)) ? LeaveStatus.Approved : LeaveStatus.ApprovedLv1; return; }
            leave.RequestStatus = LeaveStatus.Pending;
        }
    }
}
