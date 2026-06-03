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

    // Rule constants
    private const decimal MAX_WEEKLY_HOURS = 40m;
    private const decimal MAX_YEARLY_HOURS = 300m;

    public OTService(
        FVNWEBAPPContext db,
        ILogger<OTService> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _db = db;
    }

    // ========== CREATE ==========
    public async Task<ServiceResult<OTRequestDto>> CreateAsync(
        CreateOTRequestModel model,
        CurrentUser user,
        CancellationToken ct = default)
    {
        try
        {
            Logger.LogDebugIf(Debug, "[OT] Create start: {User}", user.UserName);

            // 1. Validate basic
            if (model.Employees == null || model.Employees.Count == 0)
                return ServiceResult<OTRequestDto>.Fail("Phải có ít nhất 1 nhân viên OT.");

            // 2. Kiểm tra rule giờ OT
            var ruleCheck = await CheckOTHoursRuleAsync(model.OTDate, model.Employees, ct);
            if (!ruleCheck.IsValid)
            {
                var msg = string.Join("; ", ruleCheck.Warnings.Select(w => w.Message));
                return ServiceResult<OTRequestDto>.Fail($"Vi phạm quy định giờ OT: {msg}");
            }

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var entity = new F03OTRequest
            {
                OTDate = model.OTDate,
                OTType = model.OTType,
                DeptCode = model.DeptCode,
                Reason = model.Reason,
                RequestStatus = LeaveStatus.Pending,
                TotalHours = model.Employees.Sum(e => e.PlannedHours),
                IsActive = true,
                CreatedBy = user.UserId,
                CreatedAt = DateTime.Now,
                ModifiedBy = user.UserId,
                ModifiedAt = DateTime.Now,

                Level1ApproveCode = model.Level1ApproveCode,
                Level1ApproveName = model.Level1ApproveName,
                Level1ApproveEmail = model.Level1ApproveEmail,
                Level2ApproveCode = model.Level2ApproveCode,
                Level2ApproveName = model.Level2ApproveName,
                Level2ApproveEmail = model.Level2ApproveEmail,
                Level3ApproveCode = model.Level3ApproveCode,
                Level3ApproveName = model.Level3ApproveName,
                Level3ApproveEmail = model.Level3ApproveEmail,
            };

            _db.F03OTRequests.Add(entity);
            await _db.SaveChangesAsync(ct);

            var empEntities = model.Employees.Select(e => new F03OTEmployee
            {
                OTRequestId = entity.Id,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                PlannedFrom = e.PlannedFrom,
                PlannedTo = e.PlannedTo,
                PlannedHours = e.PlannedHours,
                OTReason = e.OTReason,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            _db.F03OTEmployees.AddRange(empEntities);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            Logger.LogInfoIf(Debug, "[OT] Created Id={Id}", entity.Id);

            var dto = await BuildDtoAsync(entity.Id, ct);
            return ServiceResult<OTRequestDto>.Ok(dto!);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] Create ERROR");
            return ServiceResult<OTRequestDto>.Fail("Lỗi hệ thống khi tạo đơn OT.");
        }
    }

    // ========== RULE CHECK: 40h/tuần, 300h/năm ==========
    public async Task<OTValidationResult> CheckOTHoursRuleAsync(
        DateOnly otDate,
        List<OTEmployeeModel> employees,
        CancellationToken ct = default)
    {
        var result = new OTValidationResult { IsValid = true };

        // Tính khoảng tuần hiện tại (Thứ 2 - CN)
        var dayOfWeek = (int)otDate.DayOfWeek;
        var monday = otDate.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
        var sunday = monday.AddDays(6);

        // Khoảng năm
        var yearStart = new DateOnly(otDate.Year, 1, 1);
        var yearEnd = new DateOnly(otDate.Year, 12, 31);

        var empCodes = employees.Select(e => e.EmployeeCode).ToList();

        // Lấy OT đã được duyệt (Approved) trong tuần và năm
        var existingOT = await _db.F03OTEmployees
            .AsNoTracking()
            .Where(e =>
                empCodes.Contains(e.EmployeeCode) &&
                e.IsActive == true &&
                e.OTRequest.IsActive == true &&
                e.OTRequest.RequestStatus != "Rejected" &&
                e.OTRequest.RequestStatus != "Cancelled" &&
                e.OTRequest.OTDate >= yearStart &&
                e.OTRequest.OTDate <= yearEnd)
            .Select(e => new
            {
                e.EmployeeCode,
                e.PlannedHours,
                OTDate = e.OTRequest.OTDate
            })
            .ToListAsync(ct);

        foreach (var emp in employees)
        {
            var empOT = existingOT.Where(x => x.EmployeeCode == emp.EmployeeCode).ToList();

            decimal weeklyUsed = empOT
                .Where(x => x.OTDate >= monday && x.OTDate <= sunday)
                .Sum(x => x.PlannedHours);

            decimal yearlyUsed = empOT.Sum(x => x.PlannedHours);

            decimal weeklyTotal = weeklyUsed + emp.PlannedHours;
            decimal yearlyTotal = yearlyUsed + emp.PlannedHours;

            bool exceedsWeekly = weeklyTotal > MAX_WEEKLY_HOURS;
            bool exceedsYearly = yearlyTotal > MAX_YEARLY_HOURS;

            if (exceedsWeekly || exceedsYearly)
            {
                result.IsValid = false;
                var msgs = new List<string>();

                if (exceedsWeekly)
                    msgs.Add($"vượt 40h/tuần (hiện {weeklyTotal:F1}h)");
                if (exceedsYearly)
                    msgs.Add($"vượt 300h/năm (hiện {yearlyTotal:F1}h)");

                result.Warnings.Add(new OTHoursWarning
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName ?? emp.EmployeeCode,
                    WeeklyHours = weeklyTotal,
                    MonthlyHours = yearlyTotal,
                    ExceedsWeekly = exceedsWeekly,
                    ExceedsMonthly = exceedsYearly,
                    Message = $"{emp.EmployeeName ?? emp.EmployeeCode}: {string.Join(", ", msgs)}"
                });
            }
        }

        return result;
    }

    // ========== APPROVE / REJECT ==========
    public async Task<ServiceResult> ApproveAsync(
        List<int> ids, int level,
        CurrentUser user, string? comment,
        CancellationToken ct = default)
        => await ProcessBatchAsync(ids, level, user, comment, true, ct);

    public async Task<ServiceResult> RejectAsync(
        List<int> ids, int level,
        CurrentUser user, string? comment,
        CancellationToken ct = default)
        => await ProcessBatchAsync(ids, level, user, comment, false, ct);

    private async Task<ServiceResult> ProcessBatchAsync(
        List<int> ids, int level,
        CurrentUser user, string? comment,
        bool isApprove, CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
            return ServiceResult.Fail("Không có đơn nào được chọn.");

        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var list = await _db.F03OTRequests
                .Where(x => ids.Contains(x.Id) && x.IsActive == true)
                .ToListAsync(ct);

            bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();
            int success = 0;
            var now = DateTime.Now;

            foreach (var req in list)
            {
                if (req.RequestStatus == LeaveStatus.Approved
                 || req.RequestStatus == LeaveStatus.Rejected) continue;

                string? approverCode = level switch
                {
                    1 => req.Level1ApproveCode,
                    2 => req.Level2ApproveCode,
                    3 => req.Level3ApproveCode,
                    _ => null
                };

                if (!isAdmin && user.EmployeeCode != approverCode) continue;

                if (isApprove)
                {
                    ApplyApprove(req, level, comment, now);
                    UpdateStatus(req);
                }
                else
                {
                    ApplyReject(req, level, comment, now);
                    req.RequestStatus = LeaveStatus.Rejected;
                }
                success++;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return success > 0
                ? ServiceResult.Ok($"Đã xử lý {success}/{list.Count} đơn OT.")
                : ServiceResult.Fail("Không có đơn nào đủ điều kiện.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT] ProcessBatch ERROR");
            return ServiceResult.Fail("Lỗi hệ thống khi xử lý đơn OT.");
        }
    }

    // ========== CANCEL ==========
    public async Task<ServiceResult> CancelAsync(
        int id, CurrentUser user, CancellationToken ct = default)
    {
        var req = await _db.F03OTRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (req == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");
        if (req.RequestStatus == LeaveStatus.Approved)
            return ServiceResult.Fail("Đơn đã duyệt không thể hủy.");

        req.IsActive = false;
        req.RequestStatus = "Cancelled";
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã hủy đơn OT.");
    }

    // ========== QUERIES ==========
    public async Task<ServiceResult<OTRequestDto>> GetByIdAsync(
        int id, CancellationToken ct = default)
    {
        var dto = await BuildDtoAsync(id, ct);
        return dto == null
            ? ServiceResult<OTRequestDto>.Fail("Không tìm thấy đơn OT.")
            : ServiceResult<OTRequestDto>.Ok(dto);
    }

    public async Task<ServiceResult<List<OTRequestDto>>> GetByEmployeeAsync(
        string employeeCode, int? year,
        CancellationToken ct = default)
    {
        var ids = await _db.F03OTEmployees
            .AsNoTracking()
            .Where(e =>
                e.EmployeeCode == employeeCode &&
                e.IsActive == true &&
                (year == null || e.OTRequest.OTDate.Year == year))
            .Select(e => e.OTRequestId)
            .Distinct()
            .ToListAsync(ct);

        var dtos = new List<OTRequestDto>();
        foreach (var id in ids)
        {
            var dto = await BuildDtoAsync(id, ct);
            if (dto != null) dtos.Add(dto);
        }
        return ServiceResult<List<OTRequestDto>>.Ok(dtos);
    }

    public async Task<ServiceResult<List<OTRequestDto>>> GetPendingForApproverAsync(
        string approverEmail, CancellationToken ct = default)
    {
        var ids = await _db.F03OTRequests
            .AsNoTracking()
            .Where(r =>
                r.IsActive == true &&
                r.RequestStatus != LeaveStatus.Approved &&
                r.RequestStatus != LeaveStatus.Rejected &&
                r.RequestStatus != "Cancelled" &&
                (r.Level1ApproveEmail == approverEmail ||
                 r.Level2ApproveEmail == approverEmail ||
                 r.Level3ApproveEmail == approverEmail))
            .Select(r => r.Id)
            .ToListAsync(ct);

        var dtos = new List<OTRequestDto>();
        foreach (var id in ids)
        {
            var dto = await BuildDtoAsync(id, ct);
            if (dto != null) dtos.Add(dto);
        }
        return ServiceResult<List<OTRequestDto>>.Ok(dtos);
    }

    // ========== HELPERS ==========
    private async Task<OTRequestDto?> BuildDtoAsync(int id, CancellationToken ct)
    {
        var req = await _db.F03OTRequests
            .AsNoTracking()
            .Include(r => r.Employees)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (req == null) return null;

        var dept = await _db.F03departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeptCode == req.DeptCode, ct);

        return new OTRequestDto
        {
            Id = req.Id,
            OTDate = req.OTDate,
            OTType = req.OTType,
            DeptCode = req.DeptCode,
            DeptName = dept?.DeptName,
            Reason = req.Reason,
            RequestStatus = req.RequestStatus,
            TotalHours = req.TotalHours,
            EmployeeCount = req.Employees.Count(e => e.IsActive),
            Level1ApproveName = req.Level1ApproveName,
            Level1IsApprove = req.Level1IsApprove,
            Level2ApproveName = req.Level2ApproveName,
            Level2IsApprove = req.Level2IsApprove,
            Level3ApproveName = req.Level3ApproveName,
            Level3IsApprove = req.Level3IsApprove,
            Employees = req.Employees
                .Where(e => e.IsActive)
                .Select(e => new OTEmployeeDto
                {
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    PlannedFrom = e.PlannedFrom,
                    PlannedTo = e.PlannedTo,
                    PlannedHours = e.PlannedHours,
                    ActualFrom = e.ActualFrom,
                    ActualTo = e.ActualTo,
                    ActualHours = e.ActualHours,
                    OTReason = e.OTReason
                }).ToList()
        };
    }

    private static void ApplyApprove(F03OTRequest r, int level, string? comment, DateTime now)
    {
        if (level == 1) { r.Level1IsApprove = true; r.Level1ApproveTime = now; r.Level1Comment = comment; }
        if (level == 2) { r.Level2IsApprove = true; r.Level2ApproveTime = now; r.Level2Comment = comment; }
        if (level == 3) { r.Level3IsApprove = true; r.Level3ApproveTime = now; r.Level3Comment = comment; }
    }

    private static void ApplyReject(F03OTRequest r, int level, string? comment, DateTime now)
    {
        if (level == 1) { r.Level1IsApprove = false; r.Level1ApproveTime = now; r.Level1Comment = comment; }
        if (level == 2) { r.Level2IsApprove = false; r.Level2ApproveTime = now; r.Level2Comment = comment; }
        if (level == 3) { r.Level3IsApprove = false; r.Level3ApproveTime = now; r.Level3Comment = comment; }
    }

    private static void UpdateStatus(F03OTRequest r)
    {
        bool has3 = !string.IsNullOrEmpty(r.Level3ApproveEmail);
        bool has2 = !string.IsNullOrEmpty(r.Level2ApproveEmail);

        if (has3 && r.Level3IsApprove == true) { r.RequestStatus = LeaveStatus.Approved; return; }
        if (r.Level2IsApprove == true)
        {
            r.RequestStatus = has3 ? LeaveStatus.ApprovedLv2 : LeaveStatus.Approved; return;
        }
        if (r.Level1IsApprove == true)
        {
            r.RequestStatus = has2 || has3 ? LeaveStatus.ApprovedLv1 : LeaveStatus.Approved; return;
        }
        r.RequestStatus = LeaveStatus.Pending;
    }
}