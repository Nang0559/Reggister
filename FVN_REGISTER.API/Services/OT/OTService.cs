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

    // ============================================================
    // CREATE
    // ============================================================
    public async Task<ServiceResult> CreateAsync(
        CreateOTRequestModel model,
        CurrentUser user,
        CancellationToken ct = default)
    {
        try
        {
            Logger.LogDebugIf(Debug, "[OT] Create start: {User}", user.UserName);

            var valResult = await _validator.ValidateAsync(model, user, ct);
            if (!valResult.IsSuccess) return valResult;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var request = new F03OTRequest
            {
                WorkYear = model.OTDate.Year,
                EmployeeCode = user.EmployeeCode!,
                DeptCode = user.DeptCode!,
                CvCode = user.CvCode,
                OTDate = DateOnly.FromDateTime(model.OTDate),
                PlannedFrom = model.PlannedFrom,
                PlannedTo = model.PlannedTo,
                PlannedHours = model.PlannedHours,
                DayType = model.DayType,
                OTReason = model.OTReason,
                RequiresGM = model.RequiresGM,
                CreatedByEmail = user.Email,
                CreatedByLevel = model.CreatedByLevel,

                // Bước 3
                Lv3ApproveCode = model.Lv3Skip ? null : model.Lv3ApproveCode,
                Lv3ApproveName = model.Lv3Skip ? null : model.Lv3ApproveName,
                Lv3ApproveEmail = model.Lv3Skip ? null : model.Lv3ApproveEmail,
                Lv3Skip = model.Lv3Skip,

                // Bước 4
                Lv4ApproveCode = model.Lv4ApproveCode,
                Lv4ApproveName = model.Lv4ApproveName,
                Lv4ApproveEmail = model.Lv4ApproveEmail,

                // Bước 5
                Lv5ApproveCode = model.Lv5ApproveCode,
                Lv5ApproveName = model.Lv5ApproveName,
                Lv5ApproveEmail = model.Lv5ApproveEmail,

                // Bước 6
                Lv6ApproveCode = model.Lv6ApproveCode,
                Lv6ApproveName = model.Lv6ApproveName,
                Lv6ApproveEmail = model.Lv6ApproveEmail,

                // Bước 7 (GM)
                Lv7ApproveCode = model.RequiresGM ? model.Lv7ApproveCode : null,
                Lv7ApproveName = model.RequiresGM ? model.Lv7ApproveName : null,
                Lv7ApproveEmail = model.RequiresGM ? model.Lv7ApproveEmail : null,

                RequestStatus = OTStatus.Pending,
                IsActive = true,
                CreatedBy = user.UserId,
                CreatedAt = DateTime.Now
            };

            _db.Set<F03OTRequest>().Add(request);
            await _db.SaveChangesAsync(ct);

            // Thêm từng nhân viên OT
            foreach (var emp in model.Employees)
            {
                var row = new F03OTRow
                {
                    OTRequestId = request.Id,
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName,
                    DeptCode = emp.DeptCode,
                    PlannedFrom = emp.PlannedFrom ?? model.PlannedFrom,
                    PlannedTo = emp.PlannedTo ?? model.PlannedTo,
                    PlannedHours = emp.PlannedHours ?? model.PlannedHours,
                };
                _db.Set<F03OTRow>().Add(row);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            Logger.LogInfoIf(Debug, "[OT] Created Id={Id}", request.Id);

            // Gửi email cho người duyệt đầu tiên
            _ = Task.Run(async () =>
            {
                try
                {
                    var firstEmail = request.Lv3Skip
                        ? request.Lv4ApproveEmail
                        : request.Lv3ApproveEmail;
                    var stepName = request.Lv3Skip ? "BCH Công đoàn" : "Sub.Leader/Leader";
                    await _notification.SendApprovalRequestAsync(request, firstEmail, stepName);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[OT] Notification failed");
                }
            }, ct);

            return ServiceResult.Ok("Đã gửi đơn OT thành công.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Create error");
            return ServiceResult.Fail("Lỗi hệ thống khi tạo đơn OT.");
        }
    }

    // ============================================================
    // APPROVE  (level = 3|4|5|6|7)
    // ============================================================
    public async Task<ServiceResult> ApproveAsync(
        List<int> ids, int level, CurrentUser user,
        string? comment, CancellationToken ct = default)
    {
        if (ids.Count == 0) return ServiceResult.Fail("Không có đơn nào được chọn.");

        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var requests = await _db.Set<F03OTRequest>()
                .Where(x => ids.Contains(x.Id) && x.IsActive)
                .ToListAsync(ct);

            int success = 0;
            var now = DateTime.Now;
            bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

            foreach (var r in requests)
            {
                // Kiểm tra quyền duyệt
                var approverEmail = GetApproverEmail(r, level);
                if (!isAdmin && user.Email != approverEmail) continue;

                // Áp dụng duyệt
                ApplyApprove(r, level, user, comment, now, isAdmin);

                // Cập nhật trạng thái tổng thể
                UpdateStatus(r);

                // Nếu Approved hoàn tất → cập nhật summary giờ OT
                if (r.RequestStatus == OTStatus.Approved)
                    await UpdateOTSummaryAsync(r, ct);

                success++;

                // Gửi email bước tiếp theo
                var (nextEmail, nextStep) = GetNextApprover(r);
                if (!string.IsNullOrEmpty(nextEmail))
                {
                    _ = Task.Run(() =>
                        _notification.SendApprovalRequestAsync(r, nextEmail, nextStep));
                }
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return success > 0
                ? ServiceResult.Ok($"Đã duyệt {success}/{requests.Count} đơn.")
                : ServiceResult.Fail("Không có đơn nào đủ điều kiện để duyệt.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Approve error");
            return ServiceResult.Fail("Lỗi hệ thống khi duyệt đơn OT.");
        }
    }

    // ============================================================
    // REJECT
    // ============================================================
    public async Task<ServiceResult> RejectAsync(
        List<int> ids, int level, CurrentUser user,
        string comment, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return ServiceResult.Fail("Phải nhập lý do từ chối.");

        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var requests = await _db.Set<F03OTRequest>()
                .Where(x => ids.Contains(x.Id) && x.IsActive)
                .ToListAsync(ct);

            int success = 0;
            var now = DateTime.Now;

            foreach (var r in requests)
            {
                if (r.RequestStatus == OTStatus.Approved) continue;

                ApplyReject(r, level, user, comment, now);
                r.RequestStatus = OTStatus.Rejected;
                success++;

                _ = Task.Run(() =>
                    _notification.SendRejectedAsync(r, user.FullName ?? user.UserName ?? ""));
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return success > 0
                ? ServiceResult.Ok($"Đã từ chối {success} đơn.")
                : ServiceResult.Fail("Không có đơn nào được xử lý.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Reject error");
            return ServiceResult.Fail("Lỗi hệ thống khi từ chối đơn.");
        }
    }

    // ============================================================
    // CANCEL
    // ============================================================
    public async Task<ServiceResult> CancelAsync(
        int id, string reason, CurrentUser user, CancellationToken ct = default)
    {
        try
        {
            var r = await _db.Set<F03OTRequest>().FindAsync([id], ct);
            if (r == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");

            bool isOwner = r.EmployeeCode == user.EmployeeCode;
            bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

            if (!isOwner && !isAdmin) return ServiceResult.Fail("Không có quyền hủy đơn này.");
            if (r.RequestStatus == OTStatus.Approved && !isAdmin)
                return ServiceResult.Fail("Đơn đã duyệt hoàn tất, không thể hủy.");

            r.RequestStatus = OTStatus.Cancelled;
            r.IsActive = false;
            r.Lv3Comment = $"Hủy bởi {(isOwner ? "NV" : "Admin")}: {reason}";
            r.ModifiedBy = user.UserId;
            r.ModifiedAt = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return ServiceResult.Ok("Đã hủy đơn OT.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Cancel error");
            return ServiceResult.Fail("Lỗi khi hủy đơn.");
        }
    }

    // ============================================================
    // CONFIRM ACTUAL HOURS (nhân viên xác nhận sau OT)
    // ============================================================
    public async Task<ServiceResult> ConfirmActualHoursAsync(
        int id, DateTime actualFrom, DateTime actualTo,
        CurrentUser user, CancellationToken ct = default)
    {
        try
        {
            var r = await _db.Set<F03OTRequest>().FindAsync([id], ct);
            if (r == null) return ServiceResult.Fail("Không tìm thấy đơn.");
            if (r.EmployeeCode != user.EmployeeCode) return ServiceResult.Fail("Không có quyền xác nhận.");
            if (r.RequestStatus != OTStatus.Approved) return ServiceResult.Fail("Đơn chưa được duyệt hoàn tất.");

            r.ActualFrom = actualFrom;
            r.ActualTo = actualTo;
            r.ActualHours = (decimal)(actualTo - actualFrom).TotalHours;
            r.EmployeeConfirmed = true;
            r.ConfirmedAt = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return ServiceResult.Ok("Đã xác nhận giờ OT thực tế.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Confirm error");
            return ServiceResult.Fail("Lỗi khi xác nhận.");
        }
    }

    // ============================================================
    // GET DETAILS
    // ============================================================
    public async Task<ServiceResult<OTDetailViewModel>> GetDetailsAsync(
        int id, CancellationToken ct = default)
    {
        var r = await _db.Set<VF03OTRequest>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r == null) return ServiceResult<OTDetailViewModel>.Fail("Không tìm thấy.");

        var rows = await _db.Set<F03OTRow>()
            .Where(x => x.OTRequestId == id && x.IsActive)
            .ToListAsync(ct);

        var vm = new OTDetailViewModel
        {
            Id = r.Id,
            EmployeeCode = r.EmployeeCode,
            EmployeeName = r.EmployeeName,
            DeptName = r.DeptName,
            OTDate = r.OTDate,
            DayType = r.DayType,
            PlannedHours = r.PlannedHours,
            ActualHours = r.ActualHours,
            OTReason = r.OTReason,
            RequiresGM = r.RequiresGM,
            RequestStatus = r.RequestStatus,
            EmployeeConfirmed = r.EmployeeConfirmed,

            ApprovalSteps = BuildSteps(r),

            Employees = rows.Select(row => new OTEmployeeModel
            {
                EmployeeCode = row.EmployeeCode,
                EmployeeName = row.EmployeeName,
                DeptCode = row.DeptCode,
                PlannedFrom = row.PlannedFrom,
                PlannedTo = row.PlannedTo,
                PlannedHours = row.PlannedHours,
                ActualFrom = row.ActualFrom,
                ActualTo = row.ActualTo,
                ActualHours = row.ActualHours
            }).ToList()
        };

        return ServiceResult<OTDetailViewModel>.Ok(vm);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private static void ApplyApprove(
        F03OTRequest r, int level, CurrentUser user,
        string? comment, DateTime now, bool isAdmin)
    {
        var suffix = isAdmin ? $" [Admin: {user.FullName}]" : "";
        switch (level)
        {
            case 3: r.Lv3IsApprove = true; r.Lv3ApproveTime = now; r.Lv3Comment = comment + suffix; break;
            case 4: r.Lv4IsApprove = true; r.Lv4ApproveTime = now; r.Lv4Comment = comment + suffix; break;
            case 5: r.Lv5IsApprove = true; r.Lv5ApproveTime = now; r.Lv5Comment = comment + suffix; break;
            case 6: r.Lv6IsApprove = true; r.Lv6ApproveTime = now; r.Lv6Comment = comment + suffix; break;
            case 7: r.Lv7IsApprove = true; r.Lv7ApproveTime = now; r.Lv7Comment = comment + suffix; break;
        }
    }

    private static void ApplyReject(
        F03OTRequest r, int level, CurrentUser user, string comment, DateTime now)
    {
        switch (level)
        {
            case 3: r.Lv3IsApprove = false; r.Lv3ApproveTime = now; r.Lv3Comment = comment; break;
            case 4: r.Lv4IsApprove = false; r.Lv4ApproveTime = now; r.Lv4Comment = comment; break;
            case 5: r.Lv5IsApprove = false; r.Lv5ApproveTime = now; r.Lv5Comment = comment; break;
            case 6: r.Lv6IsApprove = false; r.Lv6ApproveTime = now; r.Lv6Comment = comment; break;
            case 7: r.Lv7IsApprove = false; r.Lv7ApproveTime = now; r.Lv7Comment = comment; break;
        }
    }

    private static void UpdateStatus(F03OTRequest r)
    {
        // Lv3 ok (hoặc skip) → sang Lv4
        bool lv3Done = r.Lv3Skip || r.Lv3IsApprove == true;
        if (!lv3Done) { r.RequestStatus = OTStatus.Pending; return; }

        if (r.Lv4IsApprove != true) { r.RequestStatus = OTStatus.ApprovedLv3; return; }
        if (r.Lv5IsApprove != true) { r.RequestStatus = OTStatus.ApprovedLv4; return; }
        if (r.Lv6IsApprove != true) { r.RequestStatus = OTStatus.ApprovedLv5; return; }

        // Lv6 xong
        if (r.RequiresGM && r.Lv7IsApprove != true)
        { r.RequestStatus = OTStatus.ApprovedLv6; return; }

        r.RequestStatus = OTStatus.Approved;
    }

    private static string? GetApproverEmail(F03OTRequest r, int level) => level switch
    {
        3 => r.Lv3ApproveEmail,
        4 => r.Lv4ApproveEmail,
        5 => r.Lv5ApproveEmail,
        6 => r.Lv6ApproveEmail,
        7 => r.Lv7ApproveEmail,
        _ => null
    };

    private static (string? email, string stepName) GetNextApprover(F03OTRequest r)
    {
        if (!r.Lv3Skip && r.Lv3IsApprove == true && r.Lv4IsApprove == null)
            return (r.Lv4ApproveEmail, "BCH Công đoàn");
        if (r.Lv4IsApprove == true && r.Lv5IsApprove == null)
            return (r.Lv5ApproveEmail, "Ast.Chief/Chief");
        if (r.Lv5IsApprove == true && r.Lv6IsApprove == null)
            return (r.Lv6ApproveEmail, "A.MG/MG");
        if (r.Lv6IsApprove == true && r.RequiresGM && r.Lv7IsApprove == null)
            return (r.Lv7ApproveEmail, "GM");
        return (null, "");
    }

    private static List<OTApprovalStep> BuildSteps(VF03OTRequest r) =>
    [
        new() {
            Level = 3, StepName = "Sub.Leader / Leader",
            ApproverName = r.Lv3ApproveName, ApproverEmail = r.Lv3ApproveEmail,
            IsApproved = r.Lv3IsApprove, ApproveTime = r.Lv3ApproveTime,
            Comment = r.Lv3Comment, IsSkipped = r.Lv3Skip, IsRequired = !r.Lv3Skip
        },
        new() {
            Level = 4, StepName = "BCH Công đoàn",
            ApproverName = r.Lv4ApproveName, ApproverEmail = r.Lv4ApproveEmail,
            IsApproved = r.Lv4IsApprove, ApproveTime = r.Lv4ApproveTime,
            Comment = r.Lv4Comment, IsRequired = true
        },
        new() {
            Level = 5, StepName = "Ast.Chief / Chief",
            ApproverName = r.Lv5ApproveName, ApproverEmail = r.Lv5ApproveEmail,
            IsApproved = r.Lv5IsApprove, ApproveTime = r.Lv5ApproveTime,
            Comment = r.Lv5Comment, IsRequired = true
        },
        new() {
            Level = 6, StepName = "A.MG / MG",
            ApproverName = r.Lv6ApproveName, ApproverEmail = r.Lv6ApproveEmail,
            IsApproved = r.Lv6IsApprove, ApproveTime = r.Lv6ApproveTime,
            Comment = r.Lv6Comment, IsRequired = true
        },
        new() {
            Level = 7, StepName = "GM",
            ApproverName = r.Lv7ApproveName, ApproverEmail = r.Lv7ApproveEmail,
            IsApproved = r.Lv7IsApprove, ApproveTime = r.Lv7ApproveTime,
            Comment = r.Lv7Comment, IsRequired = r.RequiresGM,
            IsSkipped = !r.RequiresGM
        }
    ];

    private async Task UpdateOTSummaryAsync(F03OTRequest r, CancellationToken ct)
    {
        var rows = await _db.Set<F03OTRow>()
            .Where(x => x.OTRequestId == r.Id && x.IsActive)
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            var s = await _db.Set<F03OTSummary>()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeCode == row.EmployeeCode &&
                    x.WorkYear == r.OTDate.Year &&
                    x.WorkMonth == r.OTDate.Month, ct);

            if (s == null)
            {
                s = new F03OTSummary
                {
                    EmployeeCode = row.EmployeeCode,
                    WorkYear = r.OTDate.Year,
                    WorkMonth = r.OTDate.Month
                };
                _db.Set<F03OTSummary>().Add(s);
            }

            s.TotalHoursMonth += row.PlannedHours;
            s.TotalHoursYear += row.PlannedHours;
            s.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync(ct);
    }
}