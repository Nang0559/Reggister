// ============================================================
// FILE: FVN_REGISTER.API/Services/OT/OTValidator.cs
// Flow: Validate giờ hạn cuối - Ngày 4h, Tháng 40h, Năm 200h/900h
// CVCode 0003 = công nhân cần Bước 3 (Level1 bắt buộc)
// ============================================================
using FVN_REGISTER.Contract.Dtos;
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

public class OTValidator : BaseService<OTValidator>, IOTValidator
{
    private readonly FVNWEBAPPContext _db;

    public OTValidator(
        FVNWEBAPPContext db,
        ILogger<OTValidator> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _db = db;
    }

    public async Task<ServiceResult> ValidateCreateAsync(
        CreateOTRequestModel model,
        CurrentUser user,
        CancellationToken ct = default)
    {
        // ── 1. BASIC ──────────────────────────────────────────
        if (model.Employees == null || model.Employees.Count == 0)
            return Fail("Phải chọn ít nhất 1 nhân viên.");

        if (model.OTDate.DayOfWeek == DayOfWeek.Sunday && model.OTType == OTTypeConst.Weekday)
            return Fail("Chủ nhật không thể đăng ký OT dạng ngày thường.");

        if (model.StartTime >= model.EndTime)
            return Fail("Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");

        // Tính lại giờ thực tế từ StartTime / EndTime
        var computedHours = (decimal)(model.EndTime - model.StartTime).TotalHours;
        if (Math.Abs(computedHours - model.PlannedHours) > 0.1m)
            return Fail($"Số giờ OT ({model.PlannedHours}h) không khớp với khoảng thời gian ({computedHours:F1}h).");

        if (string.IsNullOrWhiteSpace(model.OTReason))
            return Fail("Lý do OT không được để trống.");

        // ── 2. APPROVER VALIDATION theo CVCode ────────────────
        bool hasWorker = model.HasWorker;

        // Với công nhân (CVCode 0003): bắt buộc đủ Level 1 (Sub-leader)
        if (hasWorker && string.IsNullOrWhiteSpace(model.Level1ApproveEmail))
            return Fail("Danh sách có công nhân (CVCode 0003) - bắt buộc chọn Bước 3 (Sub-leader/Leader).");

        // Tất cả đều bắt buộc Level 2 (Ast.Chief/Chief)
        if (string.IsNullOrWhiteSpace(model.Level2ApproveEmail))
            return Fail("Bắt buộc chọn người duyệt Bước 5 (Ast.Chief/Chief).");

        // Tất cả đều bắt buộc Level 3 (A.MG/MG)
        if (string.IsNullOrWhiteSpace(model.Level3ApproveEmail))
            return Fail("Bắt buộc chọn người duyệt Bước 6 (A.MG/MG).");

        // Level 4 (GM) - cần với ngày thường 3 bộ trở lên hoặc theo rule
        var needGM = await NeedGMApprovalAsync(model, ct);
        if (needGM && string.IsNullOrWhiteSpace(model.Level4ApproveEmail))
            return Fail("Đơn OT này cần duyệt qua GM (Bước 7). Vui lòng chọn người duyệt cấp 4.");

        // Không được tự duyệt đơn của mình
        var approverEmails = new[]
        {
            model.Level1ApproveEmail,
            model.Level2ApproveEmail,
            model.Level3ApproveEmail,
            model.Level4ApproveEmail
        };
        if (approverEmails.Any(e => !string.IsNullOrWhiteSpace(e) && e == user.Email))
            return Fail("Người tạo đơn không được tự chọn mình làm người duyệt.");

        // ── 3. VALIDATE GIỜ HẠN (Bước cuối trước GA) ──────────
        var limitRules = await _db.F03OTLimitRules
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

        var empCodes = model.Employees.Select(e => e.EmployeeCode).ToList();
        var otDate = DateOnly.FromDateTime(model.OTDate);

        // Lấy thống kê OT hiện tại của các nhân viên
        var existingOT = await _db.VF03OTSummaries
            .AsNoTracking()
            .Where(x => empCodes.Contains(x.EmployeeCode)
                     && x.OTYear == model.OTDate.Year
                     && x.RequestStatus != OTStatus.Rejected
                     && x.RequestStatus != OTStatus.Cancelled)
            .ToListAsync(ct);

        foreach (var emp in model.Employees)
        {
            var empOT = existingOT.Where(x => x.EmployeeCode == emp.EmployeeCode).ToList();

            // Daily: Ngày thường tối đa 4h
            if (model.OTType == OTTypeConst.Weekday)
            {
                var dailyLimit = limitRules.FirstOrDefault(r =>
                    r.RuleType == OTLimitType.Daily && r.OTType == OTTypeConst.Weekday)?.MaxHours ?? 4;

                var usedToday = empOT
                    .Where(x => x.OTDate == otDate)
                    .Sum(x => x.OTHours);

                if (usedToday + model.PlannedHours > dailyLimit)
                    return Fail($"NV {emp.EmployeeName ?? emp.EmployeeCode}: Ngày {model.OTDate:dd/MM} đã có {usedToday}h OT, thêm {model.PlannedHours}h sẽ vượt giới hạn {dailyLimit}h/ngày.");
            }

            // Monthly: 40h/tháng
            var monthlyLimit = limitRules.FirstOrDefault(r =>
                r.RuleType == OTLimitType.Monthly)?.MaxHours ?? 40;

            var usedMonth = empOT
                .Where(x => x.OTMonth == model.OTDate.Month)
                .Sum(x => x.OTHours);

            if (usedMonth + model.PlannedHours > monthlyLimit)
                return Fail($"NV {emp.EmployeeName ?? emp.EmployeeCode}: Tháng {model.OTDate.Month} đã có {usedMonth}h OT, thêm {model.PlannedHours}h sẽ vượt giới hạn {monthlyLimit}h/tháng.");

            // Yearly: 200h (hoặc 900h đặc biệt)
            var yearlyLimit = limitRules.FirstOrDefault(r =>
                r.RuleType == OTLimitType.Yearly)?.MaxHours ?? 200;

            var usedYear = empOT.Sum(x => x.OTHours);

            if (usedYear + model.PlannedHours > yearlyLimit)
                return Fail($"NV {emp.EmployeeName ?? emp.EmployeeCode}: Năm {model.OTDate.Year} đã có {usedYear}h OT, thêm {model.PlannedHours}h sẽ vượt giới hạn {yearlyLimit}h/năm. (Nếu là trường hợp đặc biệt tối đa 900h, liên hệ HR điều chỉnh.)");
        }

        // ── 4. OVERLAP: Kiểm tra nhân viên đã có OT cùng ngày/giờ chưa ──
        var overlapCheck = await _db.F03OTEmployees
            .AsNoTracking()
            .Where(e => empCodes.Contains(e.EmployeeCode)
                     && e.IsActive
                     && e.OTRequest.OTDate == otDate
                     && e.OTRequest.IsActive
                     && e.OTRequest.RequestStatus != OTStatus.Rejected
                     && e.OTRequest.RequestStatus != OTStatus.Cancelled)
            .Select(e => new { e.EmployeeCode, e.OTRequest.StartTime, e.OTRequest.EndTime })
            .ToListAsync(ct);

        var newStart = model.StartTime;
        var newEnd = model.EndTime;

        foreach (var overlap in overlapCheck)
        {
            var existStart = overlap.StartTime.ToTimeSpan();
            var existEnd = overlap.EndTime.ToTimeSpan();

            bool timeOverlap = newStart < existEnd && newEnd > existStart;
            if (timeOverlap)
            {
                var empName = model.Employees
                    .FirstOrDefault(e => e.EmployeeCode == overlap.EmployeeCode)?.EmployeeName
                    ?? overlap.EmployeeCode;
                return Fail($"NV {empName}: Đã có đơn OT trùng giờ ({existStart:hh\\:mm}-{existEnd:hh\\:mm}) trong ngày {model.OTDate:dd/MM/yyyy}.");
            }
        }

        Logger.LogDebugIf(Debug, "[OT-VALIDATE] Validation passed for {EmpCount} employees", model.Employees.Count);
        return ServiceResult.Ok();
    }

    // ── Helper: có cần GM duyệt không ──────────────────────────
    private async Task<bool> NeedGMApprovalAsync(
        CreateOTRequestModel model,
        CancellationToken ct)
    {
        // Rule: Ngày thường + từ 3 bộ phận trở lên → cần GM
        if (model.OTType == OTTypeConst.Weekday)
        {
            var distinctDepts = model.Employees
                .Select(e => e.DeptCode)
                .Distinct()
                .Count();

            if (distinctDepts >= 3) return true;
        }

        // Rule: Ngày cuối tuần hoặc lễ → cần GM
        if (model.OTType is OTTypeConst.Weekend or OTTypeConst.Holiday)
            return true;

        return false;
    }

    private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
}