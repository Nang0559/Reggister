// ============================================================
// FILE: FVN_REGISTER.API/Services/OT/OTService.cs
// ============================================================
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Services.OT;

public class OTService : BaseService<OTService>, IOTService
{
    private readonly FVNWEBAPPContext _db;
    private readonly IOTValidator _validator;
    private readonly IOTNotificationService _notification;

    public OTService(
        FVNWEBAPPContext db,
        IOTValidator validator,
        IOTNotificationService notification,
        ILogger<OTService> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _db = db;
        _validator = validator;
        _notification = notification;
    }

    // ── BƯỚC 1-2: TẠO ĐƠN OT ──────────────────────────────────
    public async Task<ServiceResult<int>> CreateOTAsync(
        CreateOTRequestModel model,
        CurrentUser user,
        CancellationToken ct = default)
    {
        try
        {
            Logger.LogDebugIf(Debug, "[OT] CreateOT start by {User}", user.UserName);

            // Validate
            var validation = await _validator.ValidateCreateAsync(model, user, ct);
            if (!validation.IsSuccess)
            {
                Logger.LogWarnIf(Debug, "[OT] Validation failed: {Msg}", validation.Message);
                return ServiceResult<int>.Fail(validation.Message!);
            }

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Sinh mã OT tự động
            var otCode = await GenerateOTCodeAsync(ct);

            var otRequest = new F03OTRequest
            {
                OTCode = otCode,
                DeptCode = model.DeptCode,
                OTDate = DateOnly.FromDateTime(model.OTDate),
                OTType = model.OTType,
                StartTime = TimeOnly.FromTimeSpan(model.StartTime),
                EndTime = TimeOnly.FromTimeSpan(model.EndTime),
                PlannedHours = model.PlannedHours,
                OTReason = model.OTReason,
                ScopeType = model.ScopeType,
                RequestStatus = OTStatus.Pending,

                // Level 1 - Sub-leader/Leader (chỉ khi có công nhân)
                Level1ApproveCode = model.HasWorker ? model.Level1ApproveCode : null,
                Level1ApproveName = model.HasWorker ? model.Level1ApproveName : null,
                Level1ApproveEmail = model.HasWorker ? model.Level1ApproveEmail : null,

                // Level 2 - Ast.Chief/Chief
                Level2ApproveCode = model.Level2ApproveCode,
                Level2ApproveName = model.Level2ApproveName,
                Level2ApproveEmail = model.Level2ApproveEmail,

                // Level 3 - A.MG/MG
                Level3ApproveCode = model.Level3ApproveCode,
                Level3ApproveName = model.Level3ApproveName,
                Level3ApproveEmail = model.Level3ApproveEmail,

                // Level 4 - GM (tùy điều kiện)
                Level4ApproveCode = model.Level4ApproveCode,
                Level4ApproveName = model.Level4ApproveName,
                Level4ApproveEmail = model.Level4ApproveEmail,

                IsActive = true,
                CreatedBy = user.UserId,
                CreatedAt = DateTime.Now
            };

            // Nếu không có công nhân → bỏ qua Level 1, trạng thái bắt đầu từ Level 2
            if (!model.HasWorker)
            {
                otRequest.Level1IsApprove = null; // skip - không cần
            }

            _db.F03OTRequests.Add(otRequest);
            await _db.SaveChangesAsync(ct);

            // Thêm danh sách nhân viên
            var employees = model.Employees.Select(e => new F03OTEmployee
            {
                OTRequestId = otRequest.Id,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                DeptCode = e.DeptCode,
                CvCode = e.CvCode,
                Note = e.Note,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            _db.F03OTEmployees.AddRange(employees);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);

            Logger.LogInfoIf(Debug, "[OT] Created: {Code} by {User}", otCode, user.UserName);

            // Gửi email thông báo bất đồng bộ
            _ = Task.Run(async () =>
            {
                try
                {
                    var firstApproverEmail = model.HasWorker
                        ? model.Level1ApproveEmail!
                        : model.Level2ApproveEmail!;

                    await _notification.SendApprovalRequestAsync(
                        firstApproverEmail,
                        otRequest,
                        user.FullName ?? user.UserName ?? "",
                        ct);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[OT] Notification failed for {Code}", otCode);
                }
            }, CancellationToken.None);

            return ServiceResult<int>.Ok(otRequest.Id, $"Tạo đơn OT {otCode} thành công.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] CreateOT ERROR");
            return ServiceResult<int>.Fail("Lỗi hệ thống khi tạo đơn OT.");
        }
    }

    // ── DUYỆT / TỪ CHỐI (BATCH) ────────────────────────────────
    public async Task<ServiceResult> ApproveAsync(
        List<int> ids, int level, CurrentUser user, string? comment,
        CancellationToken ct = default)
        => await ProcessBatchAsync(ids, level, user, comment, isApprove: true, ct);

    public async Task<ServiceResult> RejectAsync(
        List<int> ids, int level, CurrentUser user, string? comment,
        CancellationToken ct = default)
        => await ProcessBatchAsync(ids, level, user, comment, isApprove: false, ct);

    private async Task<ServiceResult> ProcessBatchAsync(
        List<int> ids, int level, CurrentUser user, string? comment,
        bool isApprove, CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
            return ServiceResult.Fail("Không có đơn nào được chọn.");

        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var requests = await _db.F03OTRequests
                .Where(x => ids.Contains(x.Id) && x.IsActive)
                .ToListAsync(ct);

            bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();
            int success = 0;
            var now = DateTime.Now;

            foreach (var req in requests)
            {
                // Bỏ qua đơn đã hoàn tất
                if (req.RequestStatus is OTStatus.Approved or OTStatus.Rejected or OTStatus.Cancelled)
                    continue;

                // Kiểm tra quyền duyệt
                var approverEmail = GetApproverEmail(req, level);
                if (!isAdmin && user.Email != approverEmail && user.EmployeeCode != GetApproverCode(req, level))
                    continue;

                if (isApprove)
                {
                    ApplyApprove(req, level, user, comment, now);
                    UpdateOverallStatus(req);
                }
                else
                {
                    ApplyReject(req, level, user, comment, now);
                    req.RequestStatus = OTStatus.Rejected;

                    // Thông báo từ chối
                    _ = Task.Run(async () =>
                    {
                        try { await _notification.SendStatusChangedAsync(req, "Rejected", ct); }
                        catch { /* silent */ }
                    }, CancellationToken.None);
                }

                success++;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return success > 0
                ? ServiceResult.Ok($"Đã xử lý {success}/{requests.Count} đơn OT.")
                : ServiceResult.Fail("Không có đơn nào đủ điều kiện xử lý.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Batch approve/reject ERROR");
            return ServiceResult.Fail("Lỗi hệ thống khi xử lý đơn OT.");
        }
    }

    // ── HỦY ĐƠN ────────────────────────────────────────────────
    public async Task<ServiceResult> CancelAsync(
        int id, string? reason, CurrentUser user, CancellationToken ct = default)
    {
        try
        {
            var req = await _db.F03OTRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (req == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");

            bool isOwner = req.CreatedBy == user.UserId;
            bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

            if (!isOwner && !isAdmin)
                return ServiceResult.Fail("Không có quyền hủy đơn này.");

            if (req.RequestStatus == OTStatus.Approved && !isAdmin)
                return ServiceResult.Fail("Đơn đã duyệt hoàn tất không thể hủy. Liên hệ Admin.");

            req.RequestStatus = OTStatus.Cancelled;
            req.IsActive = false;
            req.Level1Comment = $"[Hủy bởi {(isOwner ? "Người tạo" : "Admin")} {user.FullName}]: {reason}";
            req.ModifiedBy = user.UserId;
            req.ModifiedAt = DateTime.Now;

            await _db.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug, "[OT] Cancelled: {Id} by {User}", id, user.UserName);
            return ServiceResult.Ok("Đã hủy đơn OT thành công.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Cancel ERROR");
            return ServiceResult.Fail("Lỗi hệ thống khi hủy đơn OT.");
        }
    }

    // ── VALIDATE & ARCHIVE (Bước 9-10) ─────────────────────────
    public async Task<ServiceResult> ValidateAndArchiveAsync(
        int id, CurrentUser user, string? note, CancellationToken ct = default)
    {
        try
        {
            var req = await _db.F03OTRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (req == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");

            if (req.RequestStatus != OTStatus.Approved)
                return ServiceResult.Fail("Chỉ có thể lưu trữ đơn đã được duyệt hoàn tất.");

            req.ValidatedAt = DateTime.Now;
            req.ValidatedBy = user.UserId;
            req.ValidationNote = note;
            req.ArchivedAt = DateTime.Now;
            req.ArchivedBy = user.UserId;
            req.ModifiedBy = user.UserId;
            req.ModifiedAt = DateTime.Now;

            await _db.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug, "[OT] Archived: {Id}", id);
            return ServiceResult.Ok("Đơn OT đã được xác nhận và lưu trữ.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Archive ERROR");
            return ServiceResult.Fail("Lỗi hệ thống khi lưu trữ đơn OT.");
        }
    }

    // ── QUERIES ─────────────────────────────────────────────────
    public async Task<ServiceResult<OTRequestViewModel>> GetDetailsAsync(
        int id, CancellationToken ct = default)
    {
        try
        {
            var req = await _db.VF03OTRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (req == null)
                return ServiceResult<OTRequestViewModel>.Fail("Không tìm thấy đơn OT.");

            var employees = await _db.F03OTEmployees
                .AsNoTracking()
                .Where(e => e.OTRequestId == id && e.IsActive)
                .ToListAsync(ct);

            return ServiceResult<OTRequestViewModel>.Ok(MapToViewModel(req, employees));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] GetDetails ERROR");
            return ServiceResult<OTRequestViewModel>.Fail("Lỗi hệ thống khi lấy chi tiết đơn OT.");
        }
    }

    public async Task<ServiceResult<OTBalanceDto>> GetOTBalanceAsync(
        string employeeCode, int year, int month, CancellationToken ct = default)
    {
        try
        {
            var data = await _db.VF03OTSummaries
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                         && x.OTYear == year
                         && x.RequestStatus != OTStatus.Rejected
                         && x.RequestStatus != OTStatus.Cancelled)
                .ToListAsync(ct);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var limits = await _db.F03OTLimitRules
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(ct);

            var dto = new OTBalanceDto
            {
                EmployeeCode = employeeCode,
                Year = year,
                Month = month,
                UsedHoursToday = data.Where(x => x.OTDate == today).Sum(x => x.OTHours),
                DailyLimit = limits.FirstOrDefault(r => r.RuleType == OTLimitType.Daily)?.MaxHours ?? 4,
                UsedHoursThisMonth = data.Where(x => x.OTMonth == month).Sum(x => x.OTHours),
                MonthlyLimit = limits.FirstOrDefault(r => r.RuleType == OTLimitType.Monthly)?.MaxHours ?? 40,
                UsedHoursThisYear = data.Sum(x => x.OTHours),
                YearlyLimit = limits.FirstOrDefault(r => r.RuleType == OTLimitType.Yearly)?.MaxHours ?? 200
            };

            return ServiceResult<OTBalanceDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] GetBalance ERROR");
            return ServiceResult<OTBalanceDto>.Fail("Lỗi khi lấy số giờ OT.");
        }
    }

    // ── PRIVATE HELPERS ─────────────────────────────────────────
    private async Task<string> GenerateOTCodeAsync(CancellationToken ct)
    {
        var year = DateTime.Now.Year;
        var count = await _db.F03OTRequests.CountAsync(x => x.CreatedAt.Year == year, ct);
        return $"OT-{year}-{(count + 1):D4}";
    }

    private static void ApplyApprove(F03OTRequest req, int level, CurrentUser user, string? comment, DateTime now)
    {
        bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();
        string note = isAdmin && user.EmployeeCode != GetApproverCode(req, level)
            ? $"[Admin {user.FullName} duyệt thay]: {comment}"
            : comment ?? "";

        switch (level)
        {
            case 1: req.Level1IsApprove = true; req.Level1ApproveTime = now; req.Level1Comment = note; break;
            case 2: req.Level2IsApprove = true; req.Level2ApproveTime = now; req.Level2Comment = note; break;
            case 3: req.Level3IsApprove = true; req.Level3ApproveTime = now; req.Level3Comment = note; break;
            case 4: req.Level4IsApprove = true; req.Level4ApproveTime = now; req.Level4Comment = note; break;
        }
    }

    private static void ApplyReject(F03OTRequest req, int level, CurrentUser user, string? comment, DateTime now)
    {
        bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();
        string note = isAdmin ? $"[Admin {user.FullName} từ chối]: {comment}" : comment ?? "";

        switch (level)
        {
            case 1: req.Level1IsApprove = false; req.Level1ApproveTime = now; req.Level1Comment = note; break;
            case 2: req.Level2IsApprove = false; req.Level2ApproveTime = now; req.Level2Comment = note; break;
            case 3: req.Level3IsApprove = false; req.Level3ApproveTime = now; req.Level3Comment = note; break;
            case 4: req.Level4IsApprove = false; req.Level4ApproveTime = now; req.Level4Comment = note; break;
        }
    }

    /// <summary>
    /// Cập nhật trạng thái tổng dựa trên các level đã duyệt.
    /// Logic: Level 1 (tùy có/không) → Level 2 → Level 3 → Level 4 (tùy) → Approved
    /// </summary>
    private static void UpdateOverallStatus(F03OTRequest req)
    {
        bool hasLevel1 = !string.IsNullOrEmpty(req.Level1ApproveEmail);
        bool hasLevel4 = !string.IsNullOrEmpty(req.Level4ApproveEmail);

        bool level1Done = !hasLevel1 || req.Level1IsApprove == true;
        bool level2Done = req.Level2IsApprove == true;
        bool level3Done = req.Level3IsApprove == true;
        bool level4Done = !hasLevel4 || req.Level4IsApprove == true;

        if (level1Done && level2Done && level3Done && level4Done)
        {
            req.RequestStatus = OTStatus.Approved;
        }
        else if (level1Done && level2Done && level3Done)
        {
            req.RequestStatus = hasLevel4 ? OTStatus.ApprovedLv3 : OTStatus.Approved;
        }
        else if (level1Done && level2Done)
        {
            req.RequestStatus = OTStatus.ApprovedLv2;
        }
        else if (level1Done)
        {
            req.RequestStatus = OTStatus.ApprovedLv1;
        }
        else
        {
            req.RequestStatus = OTStatus.Pending;
        }
    }

    private static string? GetApproverEmail(F03OTRequest req, int level) => level switch
    {
        1 => req.Level1ApproveEmail,
        2 => req.Level2ApproveEmail,
        3 => req.Level3ApproveEmail,
        4 => req.Level4ApproveEmail,
        _ => null
    };

    private static string? GetApproverCode(F03OTRequest req, int level) => level switch
    {
        1 => req.Level1ApproveCode,
        2 => req.Level2ApproveCode,
        3 => req.Level3ApproveCode,
        4 => req.Level4ApproveCode,
        _ => null
    };

    private static OTRequestViewModel MapToViewModel(
        VF03OTRequest req,
        List<F03OTEmployee> employees)
    {
        var vm = new OTRequestViewModel
        {
            Id = req.Id,
            OTCode = req.OTCode,
            DeptCode = req.DeptCode,
            DeptName = req.DeptName,
            OTDate = req.OTDate.ToDateTime(TimeOnly.MinValue),
            OTType = req.OTType,
            OTTypeText = OTTypeConst.GetDisplayName(req.OTType),
            StartTime = req.StartTimeText ?? "",
            EndTime = req.EndTimeText ?? "",
            PlannedHours = req.PlannedHours,
            OTReason = req.OTReason,
            RequestStatus = req.RequestStatus,
            StatusText = req.StatusText ?? OTStatus.GetDisplayName(req.RequestStatus),
            StatusColor = req.StatusColor ?? OTStatus.GetColor(req.RequestStatus),
            EmployeeCount = req.EmployeeCount,
            CreatedAt = req.CreatedAt,
            IsActive = req.IsActive,
            Employees = employees.Select(e => new OTEmployeeModel
            {
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                DeptCode = e.DeptCode,
                CvCode = e.CvCode,
                Note = e.Note
            }).ToList()
        };

        // Build approval steps
        var steps = new List<OTApprovalStep>();

        if (!string.IsNullOrEmpty(req.Level1ApproveEmail))
        {
            steps.Add(new OTApprovalStep
            {
                Level = 1,
                RoleName = "Sub-leader / Leader",
                ApproverName = req.Level1ApproveName,
                IsApproved = req.Level1IsApprove,
                ApproveTime = req.Level1ApproveTime,
                Comment = req.Level1Comment,
                IsRequired = true // Chỉ xuất hiện khi có công nhân
            });
        }

        steps.Add(new OTApprovalStep
        {
            Level = 2,
            RoleName = "Ast. Chief / Chief",
            ApproverName = req.Level2ApproveName,
            IsApproved = req.Level2IsApprove,
            ApproveTime = req.Level2ApproveTime,
            Comment = req.Level2Comment,
            IsRequired = true
        });

        steps.Add(new OTApprovalStep
        {
            Level = 3,
            RoleName = "A.MG / MG",
            ApproverName = req.Level3ApproveName,
            IsApproved = req.Level3IsApprove,
            ApproveTime = req.Level3ApproveTime,
            Comment = req.Level3Comment,
            IsRequired = true
        });

        if (!string.IsNullOrEmpty(req.Level4ApproveEmail))
        {
            steps.Add(new OTApprovalStep
            {
                Level = 4,
                RoleName = "GM",
                ApproverName = req.Level4ApproveName,
                IsApproved = req.Level4IsApprove,
                ApproveTime = req.Level4ApproveTime,
                Comment = req.Level4Comment,
                IsRequired = true
            });
        }

        vm.ApprovalSteps = steps;
        return vm;
    }
}