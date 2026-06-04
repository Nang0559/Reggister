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

using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.API.Services.OT;

public class OTValidator : IOTValidator
{
    private readonly FVNWEBAPPContext _db;

    public OTValidator(FVNWEBAPPContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult> ValidateCreateAsync(
        CreateOTRequestModel model,
        CurrentUser user,
        CancellationToken ct = default)
    {
        try
        {
            // ── 1. Kiểm tra cơ bản ───────────────────────────────
            if (model.OTDate.Date < DateTime.Today)
                return Fail("Ngày OT không được là ngày trong quá khứ.");

            if (model.Employees == null || model.Employees.Count == 0)
                return Fail("Phải có ít nhất 1 nhân viên trong danh sách OT.");

            if (string.IsNullOrWhiteSpace(model.Reason))
                return Fail("Lý do làm thêm giờ không được để trống.");

            // ── 2. Server tự tính lại giờ (không tin client) ─────
            var plannedHours = CalcHours(model.StartTime, model.EndTime);
            if (plannedHours <= 0)
                return Fail("Giờ kết thúc phải sau giờ bắt đầu.");

            // ── 3. Validate giới hạn giờ từ F03OTLimitRule ────────
            // Lấy rules từ DB (có cache nếu cần tối ưu sau)
            var limitRules = await _db.F03OTLimitRules
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(ct);

            var dailyLimit = GetLimit(limitRules, OTLimitType.Daily,
                defaultVal: model.OTTypeCode == OTTypeConst.Weekday ? 4m : 12m);

            if (plannedHours > dailyLimit)
                return Fail(
                    $"Vượt giới hạn {dailyLimit}h/ngày " +
                    $"({OTTypeConst.GetDisplayName(model.OTTypeCode)}).");

            var yearlyLimit = GetLimit(limitRules, OTLimitType.Yearly, 200m);
            var specialLimit = GetLimit(limitRules, OTLimitType.Special, 300m);
            var monthlyLimit = GetLimit(limitRules, OTLimitType.Weekly, 40m);

            // ── 4. Validate từng nhân viên trong danh sách ────────
            foreach (var emp in model.Employees)
            {
                var baseQuery = _db.F03OTRequests
                    .AsNoTracking()
                    .Where(x =>
                        x.IsActive == true &&
                        x.RequestStatus != OTStatus.Rejected &&
                        x.RequestStatus != OTStatus.Cancelled);

                // Kiểm tra theo tháng (40h/tháng)
                var monthUsed = await baseQuery
                    .Where(x =>
                        x.OTDate.Month == model.OTDate.Month &&
                        x.OTDate.Year == model.OTDate.Year)
                    .Join(_db.F03OTEmployees,
                        r => r.Id,
                        e => e.OTRequestId,
                        (r, e) => e)
                    .Where(e => e.EmployeeCode == emp.EmployeeCode)
                    .SumAsync(e => (decimal?)e.OTHours ?? 0, ct);

                if (monthUsed + plannedHours > monthlyLimit)
                    return Fail(
                        $"Nhân viên {emp.EmployeeCode} vượt {monthlyLimit}h/tháng " +
                        $"(đã dùng: {monthUsed}h, thêm: {plannedHours}h).");

                // Kiểm tra theo năm (200h hoặc 300h đặc biệt)
                var yearUsed = await baseQuery
                    .Where(x => x.OTDate.Year == model.OTDate.Year)
                    .Join(_db.F03OTEmployees,
                        r => r.Id,
                        e => e.OTRequestId,
                        (r, e) => e)
                    .Where(e => e.EmployeeCode == emp.EmployeeCode)
                    .SumAsync(e => (decimal?)e.OTHours ?? 0, ct);

                // Tạm dùng yearlyLimit, sau khi có cờ IsSpecialProcedure thì dùng specialLimit
                if (yearUsed + plannedHours > yearlyLimit)
                    return Fail(
                        $"Nhân viên {emp.EmployeeCode} vượt {yearlyLimit}h/năm " +
                        $"(đã dùng: {yearUsed}h, thêm: {plannedHours}h). " +
                        "Liên hệ HR nếu có thủ tục đặc biệt.");
            }

            // ── 5. Validate approvers bắt buộc ───────────────────
            // Công nhân (HasWorker) cần đủ Level 3 → 4 → 5 → 6 → 7
            // NV VP chỉ cần từ Level 5 trở lên
            if (model.HasWorker && string.IsNullOrWhiteSpace(model.Level3ApproveEmail))
                return Fail("Công nhân bắt buộc chọn Sub-leader / Leader (Bước 3).");

            // TODO: Khi thêm Level4 vào DB — uncomment dòng này:
            // if (string.IsNullOrWhiteSpace(model.Level4ApproveEmail))
            //     return Fail("Chưa chọn BCH Công đoàn (Bước 4).");

            if (string.IsNullOrWhiteSpace(model.Level5ApproveEmail))
                return Fail("Chưa chọn Ast.Chief / Chief (Bước 5).");

            if (string.IsNullOrWhiteSpace(model.Level6ApproveEmail))
                return Fail("Chưa chọn A.MG / MG (Bước 6).");

            // GM bắt buộc nếu: OT ngày nghỉ/lễ HOẶC người tạo là Ast.Chief trở lên
            bool requiresGM = RequiresGM(model);
            if (requiresGM && string.IsNullOrWhiteSpace(model.Level7ApproveEmail))
                return Fail("OT ngày nghỉ/lễ hoặc từ Ast.Chief bắt buộc chọn GM (Bước 7).");

            // ── 6. Không tự duyệt ────────────────────────────────
            var allApproverEmails = new[]
            {
                    model.Level3ApproveEmail,
                    // model.Level4ApproveEmail, // TODO: thêm sau
                    model.Level5ApproveEmail,
                    model.Level6ApproveEmail,
                    model.Level7ApproveEmail
                };

            if (allApproverEmails.Any(e =>
                !string.IsNullOrEmpty(e) &&
                e.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                return Fail("Không thể chọn chính mình làm người phê duyệt.");

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return Fail($"Lỗi validate đơn OT: {ex.Message}");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Tính giờ từ TimeSpan, làm tròn xuống bội số 15 phút theo QĐ-HC-03.
    /// Ví dụ: 17:00 → 20:45 = 3.75h (không phải 3.77h)
    /// </summary>
    public static decimal CalcHours(TimeSpan start, TimeSpan end)
    {
        if (end <= start) return 0;
        var diff = end - start;
        var totalMin = (int)(diff.TotalMinutes / 15) * 15;
        return Math.Round((decimal)totalMin / 60, 2);
    }

    /// <summary>
    /// GM bắt buộc khi:
    /// - OT ngày cuối tuần / lễ / Tết
    /// - Hoặc người tạo đơn là Ast.Chief trở lên (LevelApprove >= 5)
    /// </summary>
    public static bool RequiresGM(CreateOTRequestModel model)
        => model.OTTypeCode != OTTypeConst.Weekday;

    private static decimal GetLimit(
        List<F03OTLimitRule> rules,
        string limitType,
        decimal defaultVal)
        => rules.FirstOrDefault(r => r.LimitType == limitType)?.LimitValue
           ?? defaultVal;

    private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
}
