using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Core.Entities.OT;

using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTValidator : IOTValidator
    {
        private readonly IUnitOfWork _uow;
        private readonly IApprovalProvider<OTRequestSubject> _approvalProvider;

        public OTValidator(
            IUnitOfWork uow,
            IApprovalProvider<OTRequestSubject> approvalProvider)
        {
            _uow = uow;
            _approvalProvider = approvalProvider;
        }

        // ════════════════════════════════════════════════════════════════════
        // ValidateEmployeeHoursAsync
        // — ÁP DỤNG (B): monthUsed/yearUsed ưu tiên ActualHours (đã đối chiếu
        //   qua OTAttendanceReconciliationService), fallback OTHours (đăng ký)
        //   cho các đơn chưa được đối chiếu.
        // ════════════════════════════════════════════════════════════════════
        public async Task<OTValidationResultDto> ValidateEmployeeHoursAsync(
            string employeeCode,
            DateTime otDate,
            decimal hours,
            string otTypeCode,
            CancellationToken ct = default,
            int? excludeOTRequestId = null)
        {
            var result = new OTValidationResultDto();

            try
            {
                if (hours <= 0)
                {
                    result.IsValid = false;
                    result.Errors.Add("Giờ OT phải lớn hơn 0.");
                    return result;
                }

                var otType = await GetOTTypeAsync(otTypeCode, ct);
                if (otType == null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Loại OT '{otTypeCode}' không tồn tại.");
                    return result;
                }

                var limitRules = await _uow.Repository<F03OTLimitRule>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .ToListAsync(ct);

                bool isWeekday = await IsWeekdayTypeAsync(otType, ct);
                decimal defaultDailyLimit = isWeekday ? 4m : 12m;
                decimal dailyLimit = GetLimit(limitRules, OTLimitType.Daily, defaultDailyLimit);
                decimal? weeklyLimit = GetConfiguredLimit(limitRules, OTLimitType.Weekly);
                decimal monthlyLimit = GetLimit(limitRules, OTLimitType.Monthly, 40m);
                decimal yearlyLimit = GetLimit(limitRules, OTLimitType.Yearly, 200m);
                decimal yearlySpecialLimit = GetLimit(limitRules, OTLimitType.Special, 300m);

                // Giới hạn ngày luôn so với giờ ĐANG XIN (chưa xảy ra) → không liên quan ActualHours
                if (hours > dailyLimit)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Vượt giới hạn {dailyLimit}h/ngày ({otType.OTTypeName}).");
                }

                int year = otDate.Year;
                int month = otDate.Month;
                int weekOffset = (7 + (int)otDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                DateTime weekStart = otDate.Date.AddDays(-weekOffset);
                DateTime weekEnd = weekStart.AddDays(7);

                var query = _uow.Repository<F03OTEmployee>().Query()
                    .AsNoTracking()
                    .Where(e =>
                        e.EmployeeCode == employeeCode &&
                        e.IsActive == true &&
                        e.OTRequest.IsActive == true &&
                        e.OTRequest.RequestStatus != ApprovalStatus.Rejected &&
                        e.OTRequest.RequestStatus != ApprovalStatus.Cancelled);

                if (excludeOTRequestId.HasValue)
                    query = query.Where(e => e.OTRequestId != excludeOTRequestId.Value);

                // (B) — lấy cả OTHours lẫn ActualHours, quyết định ưu tiên ở tầng C# (Select
                // ternary dịch được sang SQL CASE WHEN, không cần load thừa dữ liệu).
                var usedData = await query
                    .Select(e => new
                    {
                        EffectiveHours = (e.ActualHours.HasValue && e.ActualHours.Value > 0)
                            ? e.ActualHours.Value
                            : e.OTHours,
                        e.OTRequest.OTDate
                    })
                    .ToListAsync(ct);

                decimal weekUsed = usedData
                    .Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd)
                    .Sum(x => x.EffectiveHours);

                if (weeklyLimit.HasValue && weekUsed + hours > weeklyLimit.Value)
                {
                    result.IsValid = false;
                    result.Errors.Add(
                        $"Vượt {weeklyLimit.Value}h/tuần (đã dùng: {weekUsed}h, thêm: {hours}h).");
                }

                decimal monthUsed = usedData
                    .Where(x => x.OTDate.Month == month && x.OTDate.Year == year)
                    .Sum(x => x.EffectiveHours);

                if (monthUsed + hours > monthlyLimit)
                {
                    result.IsValid = false;
                    result.Errors.Add(
                        $"Vượt {monthlyLimit}h/tháng (đã dùng: {monthUsed}h, thêm: {hours}h).");
                }

                decimal yearUsed = usedData.Where(x => x.OTDate.Year == year).Sum(x => x.EffectiveHours);
                decimal totalYear = yearUsed + hours;

                if (totalYear > yearlySpecialLimit)
                {
                    result.IsValid = false;
                    result.Errors.Add(
                        $"Vượt giới hạn tối đa {yearlySpecialLimit}h/năm " +
                        $"(đã dùng: {yearUsed}h, thêm: {hours}h). " +
                        "Cần thủ tục đặc biệt và không thể vượt quá.");
                }
                else if (totalYear > yearlyLimit)
                {
                    result.Warnings.Add(
                        $"Đã vượt {yearlyLimit}h/năm tiêu chuẩn " +
                        $"(đã dùng: {yearUsed}h, thêm: {hours}h). " +
                        "Cần thủ tục đặc biệt theo QĐ-HC-03.");
                }

                if (result.IsValid)
                {
                    if (monthUsed + hours > monthlyLimit * 0.9m)
                        result.Warnings.Add($"Sắp đạt giới hạn tháng ({monthUsed + hours}h / {monthlyLimit}h).");

                    if (totalYear > yearlyLimit * 0.9m && totalYear <= yearlyLimit)
                        result.Warnings.Add($"Sắp đạt giới hạn năm ({totalYear}h / {yearlyLimit}h).");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Lỗi validate: {ex.Message}");
                return result;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ValidateCreateAsync — cùng nguyên tắc (B) áp cho usedData nhiều nhân viên
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult> ValidateCreateAsync(
     OTRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                if (model.Employees == null || model.Employees.Count == 0)
                    return Fail("Phải có ít nhất 1 nhân viên trong danh sách OT.");

                if (string.IsNullOrWhiteSpace(model.OTTypeCode))
                    return Fail("Phải chọn loại OT.");

                var otType = await GetOTTypeAsync(model.OTTypeCode, ct);
                if (otType == null)
                    return Fail($"Loại OT '{model.OTTypeCode}' không tồn tại hoặc đã bị vô hiệu.");

                var requestHours = CalcHours(model.StartTime, model.EndTime);   // TimeSpan — khớp đúng Dto
                if (requestHours <= 0)
                    return Fail("Giờ kết thúc phải sau giờ bắt đầu.");

                var limitRules = await _uow.Repository<F03OTLimitRule>().Query()
                    .AsNoTracking().Where(x => x.IsActive==true).ToListAsync(ct);

                bool isWeekday = await IsWeekdayTypeAsync(otType, ct);
                decimal defaultDailyLimit = isWeekday ? 4m : 12m;
                decimal dailyLimit = GetLimit(limitRules, OTLimitType.Daily, defaultDailyLimit);
                decimal? weeklyLimit = GetConfiguredLimit(limitRules, OTLimitType.Weekly);
                decimal monthlyLimit = GetLimit(limitRules, OTLimitType.Monthly, 40m);
                decimal yearlyLimit = GetLimit(limitRules, OTLimitType.Yearly, 200m);
                decimal yearlySpecialLimit = GetLimit(limitRules, OTLimitType.Special, 300m);

                // ⚠️ CHỜ XÁC NHẬN: emp.OTReasonCategoryCode không tồn tại trong OTEmployeeUpsertDto
                // (chỉ có emp.Reason string tự do). Tạm bỏ đoạn check "hạng mục lý do" —
                // KHÔI PHỤC lại đúng field thật sau khi bạn xác nhận shape Employees dùng ở đây.
                // foreach (var emp in model.Employees) { check lý do ở đây nếu cần }

                var empCodes = model.Employees.Select(e => e.EmployeeCode).ToList();
                int year = model.OTDate.Year;
                int month = model.OTDate.Month;
                int weekOffset = (7 + (int)model.OTDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                DateTime weekStart = model.OTDate.Date.AddDays(-weekOffset);
                DateTime weekEnd = weekStart.AddDays(7);

                var usedData = await _uow.Repository<F03OTEmployee>().Query()
                    .AsNoTracking()
                    .Where(e =>
                        empCodes.Contains(e.EmployeeCode) &&
                        e.OTRequest.IsActive == true &&
                        e.OTRequest.RequestStatus != ApprovalStatus.Rejected &&
                        e.OTRequest.RequestStatus != ApprovalStatus.Cancelled)
                    .Select(e => new
                    {
                        e.EmployeeCode,
                        EffectiveHours = (e.ActualHours.HasValue && e.ActualHours.Value > 0)
                            ? e.ActualHours.Value : e.OTHours,
                        e.OTRequest.OTDate
                    })
                    .ToListAsync(ct);

                foreach (var emp in model.Employees)
                {
                    // ⚠️ CHỜ XÁC NHẬN: OTEmployeeUpsertDto không có StartTime/EndTime riêng —
                    // chỉ có OTHours trực tiếp (Required, Range 0.1-24). Nghĩa là mỗi nhân viên
                    // KHÔNG tự khai giờ riêng, luôn dùng model.OTHours do chính họ nhập.
                    decimal empHours = emp.OTHours;   // SỬA — bỏ nhánh StartTime/EndTime vì Dto không có

                    if (empHours > dailyLimit)
                        return Fail($"Nhân viên {emp.EmployeeCode}: vượt giới hạn {dailyLimit}h/ngày ({otType.OTTypeName}).");

                    var empUsed = usedData.Where(x => x.EmployeeCode == emp.EmployeeCode).ToList();

                    decimal weekUsed = empUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd)
                        .Sum(x => x.EffectiveHours);

                    if (weeklyLimit.HasValue && weekUsed + empHours > weeklyLimit.Value)
                        return Fail($"Nhân viên {emp.EmployeeCode}: vượt {weeklyLimit.Value}h/tuần (đã dùng: {weekUsed}h, thêm: {empHours}h).");

                    decimal monthUsed = empUsed.Where(x => x.OTDate.Month == month && x.OTDate.Year == year)
                        .Sum(x => x.EffectiveHours);

                    if (monthUsed + empHours > monthlyLimit)
                        return Fail($"Nhân viên {emp.EmployeeCode}: vượt {monthlyLimit}h/tháng " +
                            $"(đã dùng: {monthUsed}h, thêm: {empHours}h).");

                    decimal yearUsed = empUsed.Where(x => x.OTDate.Year == year).Sum(x => x.EffectiveHours);

                    if (yearUsed + empHours > yearlySpecialLimit)
                        return Fail($"Nhân viên {emp.EmployeeCode}: vượt giới hạn tối đa {yearlySpecialLimit}h/năm " +
                            $"(đã dùng: {yearUsed}h, thêm: {empHours}h). Không thể đăng ký thêm.");

                    if (yearUsed + empHours > yearlyLimit)
                        return Fail($"Nhân viên {emp.EmployeeCode}: đã vượt {yearlyLimit}h/năm tiêu chuẩn; cần thủ tục đặc biệt trước khi đăng ký phần OT vượt chuẩn.");
                }

                // ⚠️ CHỜ XÁC NHẬN: model.ApprovalSteps do client gửi lên — dùng để làm gì?
                // Tạm giữ nguyên cách build hierarchy từ server (an toàn hơn, không tin client),
                // BỎ QUA model.ApprovalSteps cho tới khi bạn xác nhận mục đích của field này.
                var buildCtx = ApprovalBuildContext.ForOT(
                    user.EmployeeCode ?? "", model.DeptCode, user.PositionCode ?? "",
                    requestHours, model.OTTypeCode);

                var steps = await _approvalProvider.BuildHierarchyAsync(buildCtx, ct);   // SỬA: đổi tên biến, bỏ .Steps

                var missingRequired = steps
                    .Where(s => s.IsRequired && string.IsNullOrWhiteSpace(s.ApproverEmail))
                    .ToList();

                if (missingRequired.Any())
                {
                    var levelNames = string.Join(", ", missingRequired.Select(s => s.LevelName));
                    return Fail($"Phòng ban {model.DeptCode} chưa có người duyệt cấp: {levelNames}. Liên hệ Admin để cập nhật.");
                }

                if (steps.Any(s => s.IsRequired && !string.IsNullOrEmpty(s.ApproverEmail) &&
                        s.ApproverEmail!.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return Fail("Bạn không thể vừa là người tạo đơn vừa là người phê duyệt cấp này.");
                }

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                return Fail($"Lỗi validate đơn OT: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // PUBLIC HELPERS
        // ════════════════════════════════════════════════════════════════════
        public static decimal CalcHours(TimeSpan start, TimeSpan end)
        {
            if (end <= start) return 0;
            int totalMin = (int)((end - start).TotalMinutes / 15) * 15;
            return Math.Round((decimal)totalMin / 60, 2);
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════
        private async Task<F03OTType?> GetOTTypeAsync(string otTypeCode, CancellationToken ct)
            => await _uow.Repository<F03OTType>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OTTypeCode == otTypeCode && x.IsActive==true, ct);

        private async Task<bool> IsWeekdayTypeAsync(F03OTType otType, CancellationToken ct)
        {
            var minRate = await _uow.Repository<F03OTType>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true)
                .MinAsync(x => x.RateMultiplier, ct);

            return otType.RateMultiplier <= minRate;
        }

        private static decimal GetLimit(
            List<F03OTLimitRule> rules, OTLimitType limitType, decimal defaultVal)
            => rules.FirstOrDefault(r => r.LimitType == limitType)?.LimitHours
               ?? rules.FirstOrDefault(r => r.LimitType == limitType)?.LimitValue
               ?? defaultVal;

        private static decimal? GetConfiguredLimit(
            List<F03OTLimitRule> rules, OTLimitType limitType)
            => rules.FirstOrDefault(r => r.LimitType == limitType)?.LimitHours
               ?? rules.FirstOrDefault(r => r.LimitType == limitType)?.LimitValue;

        private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
    }
}