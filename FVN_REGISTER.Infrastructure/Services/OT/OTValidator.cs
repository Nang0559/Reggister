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

namespace FVN_REGISTER.Infrastructure.Services.OT
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
                    return Invalid("Giờ OT phải lớn hơn 0.");

                var otType = await GetOTTypeAsync(otTypeCode, ct);
                if (otType == null)
                    return Invalid($"Loại OT '{otTypeCode}' không tồn tại.");

                var emp = await _uow.Repository<F03Employee>().Query()
                    .AsNoTracking()
                    .Where(e => e.EmployeeCode == employeeCode && e.IsActive == true)
                    .Select(e => new { e.EmployeeCode, e.EmployeeName, e.DeptCode, e.PositionCode })
                    .FirstOrDefaultAsync(ct);

                if (emp == null)
                    return Invalid($"Không tìm thấy nhân viên {employeeCode}.");

                var rules = await _uow.Repository<F03OTLimitRule>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .ToListAsync(ct);

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

                var usedData = await query
                    .Select(e => new
                    {
                        EffectiveHours = e.ActualHours.HasValue && e.ActualHours.Value > 0
                            ? e.ActualHours.Value : e.OTHours,
                        e.OTRequest.OTDate
                    })
                    .ToListAsync(ct);

                var weekOffset = (7 + (int)otDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var weekStart = otDate.Date.AddDays(-weekOffset);
                var weekEnd = weekStart.AddDays(7);
                var monthStart = new DateTime(otDate.Year, otDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var yearStart = new DateTime(otDate.Year, 1, 1);
                var yearEnd = yearStart.AddYears(1);

                var checks = new[]
                {
                    (OTLimitType.Daily, usedData.Where(x => x.OTDate.Date == otDate.Date).Sum(x => x.EffectiveHours)),
                    (OTLimitType.Weekly, usedData.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours)),
                    (OTLimitType.Monthly, usedData.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours)),
                    (OTLimitType.Yearly, usedData.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours))
                };

                foreach (var check in checks)
                {
                    var rule = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode, check.Item1);
                    if (rule == null)
                        continue;

                    var projected = check.Item2 + hours;
                    if (projected > rule.LimitHours)
                    {
                        result.IsValid = false;
                        result.Errors.Add(
                            $"Vượt {rule.LimitHours}h/{check.Item1} (đã dùng: {check.Item2}h, thêm: {hours}h).");
                    }
                    else if (projected > rule.LimitHours * 0.9m)
                    {
                        result.Warnings.Add(
                            $"Sắp đạt giới hạn {check.Item1}: {projected}h / {rule.LimitHours}h.");
                    }
                }

                result.IsValid = result.Errors.Count == 0;
                result.Message = result.IsValid ? "Hợp lệ." : "Vượt giới hạn giờ OT.";
                return result;
            }
            catch (Exception ex)
            {
                return Invalid($"Lỗi validate: {ex.Message}");
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

                var empCodes = model.Employees.Select(e => e.EmployeeCode).Distinct().ToList();
                var employeeContexts = await _uow.Repository<F03Employee>().Query()
                    .AsNoTracking()
                    .Where(e => empCodes.Contains(e.EmployeeCode) && e.IsActive == true)
                    .Select(e => new { e.EmployeeCode, e.EmployeeName, e.DeptCode, e.PositionCode })
                    .ToListAsync(ct);

                if (employeeContexts.Count != empCodes.Count)
                    return Fail("Có nhân viên trong đơn OT không tồn tại hoặc đã ngừng hoạt động.");

                var deptCodes = employeeContexts.Select(e => e.DeptCode).Distinct().ToList();
                if (deptCodes.Count != 1)
                    return Fail("Đăng ký OT nhiều nhân viên chỉ được phép trong cùng một phòng ban.");

                var deptCode = deptCodes[0];
                var blockCode = await _uow.Repository<F03Department>().Query()
                    .AsNoTracking()
                    .Where(d => d.DeptCode == deptCode && d.IsActive == true)
                    .Select(d => d.BlockCode)
                    .FirstOrDefaultAsync(ct);

                var activeRules = await _uow.Repository<F03OTLimitRule>().Query()
                    .AsNoTracking()
                    .Where(r => r.IsActive == true)
                    .ToListAsync(ct);

                var yearStart = new DateTime(model.OTDate.Year, 1, 1);
                var yearEnd = yearStart.AddYears(1);
                var weekOffset = (7 + (int)model.OTDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var weekStart = model.OTDate.Date.AddDays(-weekOffset);
                var weekEnd = weekStart.AddDays(7);
                var monthStart = new DateTime(model.OTDate.Year, model.OTDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var blockDeptCodes = string.IsNullOrWhiteSpace(blockCode)
                    ? new List<string>()
                    : await _uow.Repository<F03Department>().Query()
                        .AsNoTracking()
                        .Where(d => d.IsActive == true && d.BlockCode == blockCode)
                        .Select(d => d.DeptCode)
                        .ToListAsync(ct);

                var usedData = await _uow.Repository<F03OTEmployee>().Query()
                    .AsNoTracking()
                    .Where(e =>
                        e.IsActive == true &&
                        e.OTRequest.IsActive == true &&
                        e.OTRequest.RequestStatus != ApprovalStatus.Rejected &&
                        e.OTRequest.RequestStatus != ApprovalStatus.Cancelled &&
                        e.OTRequest.OTDate >= yearStart &&
                        e.OTRequest.OTDate < yearEnd &&
                        (e.EmployeeCode != null || e.OTRequest.DeptCode == deptCode || blockDeptCodes.Contains(e.OTRequest.DeptCode ?? string.Empty)))
                    .Select(e => new
                    {
                        e.EmployeeCode,
                        e.OTRequest.DeptCode,
                        EffectiveHours = (e.ActualHours.HasValue && e.ActualHours.Value > 0)
                            ? e.ActualHours.Value : e.OTHours,
                        e.OTRequest.OTDate
                    })
                    .ToListAsync(ct);

                foreach (var emp in employeeContexts)
                {
                    var requested = model.Employees
                        .Where(x => x.EmployeeCode == emp.EmployeeCode)
                        .Sum(x => x.OTHours);

                    var empUsed = usedData.Where(x => x.EmployeeCode == emp.EmployeeCode).ToList();

                    ValidateScopedLimit(activeRules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode,
                        OTLimitType.Daily, empUsed.Where(x => x.OTDate.Date == model.OTDate.Date).Sum(x => x.EffectiveHours),
                        requested, model.OTDate, emp.EmployeeName);

                    ValidateScopedLimit(activeRules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode,
                        OTLimitType.Weekly, empUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours),
                        requested, model.OTDate, emp.EmployeeName);

                    ValidateScopedLimit(activeRules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode,
                        OTLimitType.Monthly, empUsed.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours),
                        requested, model.OTDate, emp.EmployeeName);

                    ValidateScopedLimit(activeRules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode,
                        OTLimitType.Yearly, empUsed.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours),
                        requested, model.OTDate, emp.EmployeeName);
                }

                var deptUsed = usedData.Where(x => x.DeptCode == deptCode).ToList();
                ValidateAggregateLimit(activeRules, OTLimitScopeType.Department, deptCode, OTLimitType.Weekly,
                    deptUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours),
                    model.Employees.Sum(x => x.OTHours));
                ValidateAggregateLimit(activeRules, OTLimitScopeType.Department, deptCode, OTLimitType.Monthly,
                    deptUsed.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours),
                    model.Employees.Sum(x => x.OTHours));
                ValidateAggregateLimit(activeRules, OTLimitScopeType.Department, deptCode, OTLimitType.Yearly,
                    deptUsed.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours),
                    model.Employees.Sum(x => x.OTHours));

                if (!string.IsNullOrWhiteSpace(blockCode))
                {
                    var blockUsed = usedData.Where(x => blockDeptCodes.Contains(x.DeptCode ?? string.Empty)).ToList();
                    ValidateAggregateLimit(activeRules, OTLimitScopeType.Block, blockCode, OTLimitType.Weekly,
                        blockUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours),
                        model.Employees.Sum(x => x.OTHours));
                    ValidateAggregateLimit(activeRules, OTLimitScopeType.Block, blockCode, OTLimitType.Monthly,
                        blockUsed.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours),
                        model.Employees.Sum(x => x.OTHours));
                    ValidateAggregateLimit(activeRules, OTLimitScopeType.Block, blockCode, OTLimitType.Yearly,
                        blockUsed.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours),
                        model.Employees.Sum(x => x.OTHours));
                }

                // ⚠️ CHỜ XÁC NHẬN: model.ApprovalSteps do client gửi lên — dùng để làm gì?
                // Tạm giữ nguyên cách build hierarchy từ server (an toàn hơn, không tin client),
                // BỎ QUA model.ApprovalSteps cho tới khi bạn xác nhận mục đích của field này.
                var buildCtx = ApprovalBuildContext.ForOT(
                    requestId: 0,
                    employeeCode: user.EmployeeCode ?? "",
                    deptCode: model.DeptCode,
                    positionCode: user.PositionCode ?? "",
                    totalOTHours: requestHours,
                    otTypeCode: model.OTTypeCode);

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

        private static void ValidateScopedLimit(
            List<F03OTLimitRule> rules,
            string employeeCode,
            string positionCode,
            string deptCode,
            OTLimitType type,
            decimal used,
            decimal requested,
            DateTime otDate,
            string employeeName)
        {
            var rule = ResolveEmployeeRule(rules, employeeCode, positionCode, deptCode, type);
            if (rule == null)
                return;

            if (used + requested > rule.LimitHours)
                throw new InvalidOperationException(
                    $"Nhân viên {employeeCode} - {employeeName}: vượt giới hạn OT {rule.LimitHours}h/{type} (đã dùng: {used}h, thêm: {requested}h).");
        }

        private static void ValidateAggregateLimit(
            List<F03OTLimitRule> rules,
            OTLimitScopeType scopeType,
            string scopeCode,
            OTLimitType type,
            decimal used,
            decimal requested)
        {
            var rule = rules.FirstOrDefault(r =>
                r.ScopeType == scopeType &&
                r.LimitType == type &&
                r.ScopeCode == scopeCode);

            if (rule == null)
                return;

            if (used + requested > rule.LimitHours)
                throw new InvalidOperationException(
                    $"{scopeType} {scopeCode}: vượt giới hạn OT {rule.LimitHours}h/{type} (đã dùng: {used}h, thêm: {requested}h).");
        }

        private static F03OTLimitRule? ResolveEmployeeRule(
            List<F03OTLimitRule> rules,
            string employeeCode,
            string? positionCode,
            string deptCode,
            OTLimitType type)
        {
            return rules
                .Where(r => r.ScopeType == OTLimitScopeType.Employee
                    && r.LimitType == type
                    && (string.IsNullOrWhiteSpace(r.EmployeeCode) || r.EmployeeCode == employeeCode)
                    && (r.DeptCode == null || r.DeptCode == deptCode)
                    && (r.PositionCode == null || r.PositionCode == positionCode))
                .OrderByDescending(r => !string.IsNullOrWhiteSpace(r.EmployeeCode))
                .ThenByDescending(r => r.PositionCode != null && r.DeptCode != null)
                .ThenByDescending(r => r.DeptCode != null)
                .ThenByDescending(r => r.PositionCode != null)
                .FirstOrDefault();
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

        private static OTValidationResultDto Invalid(string message)
        {
            return new OTValidationResultDto
            {
                IsValid = false,
                Message = message,
                Errors = new List<string> { message }
            };
        }

        private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
    }
}