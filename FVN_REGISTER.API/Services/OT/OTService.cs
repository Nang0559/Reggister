// ============================================================
// FVN_REGISTER.API/Services/OT/OTNotificationService.cs
// ============================================================
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Emails;
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

namespace FVN_REGISTER.API.Services.OT
{
   
// ============================================================
// FVN_REGISTER.API/Services/OT/OTService.cs
// ============================================================

namespace FVN_REGISTER.API.Services.OT
{
    public class OTService : BaseService<OTService>, IOTService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IOTValidator _validator;
        private readonly IOTQueryService _query;
        private readonly IOTNotificationService _notification;

        public OTService(
            FVNWEBAPPContext db,
            IOTValidator validator,
            IOTQueryService query,
            IOTNotificationService notification,
            ILogger<OTService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _validator = validator;
            _query = query;
            _notification = notification;
        }

        // ── CREATE ─────────────────────────────────────────────────────────────
        public async Task<ServiceResult<int>> CreateAsync(
            CreateOTRequestModel model,
            CurrentUser creator,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[OT] Create start: {Emp}", creator.EmployeeCode);

                // 1. Validate
                var valResult = await _validator.ValidateAsync(model, creator, ct);
                if (!valResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[OT] Validation failed: {Msg}", valResult.Message);
                    return ServiceResult<int>.Fail(valResult.Message!);
                }

                // 2. Tạo OTCode (yyyyMMdd + random suffix)
                string otCode = $"OT{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

                // 3. Xác định trạng thái ban đầu
                // Công nhân → cần Bước 3 trước; văn phòng → bắt đầu từ Bước 5
                bool hasWorker = model.HasWorker;
                string initStatus = hasWorker && !string.IsNullOrEmpty(model.Level3ApproveEmail)
                    ? OTStatus.Pending       // chờ Lv3
                    : OTStatus.Pending;      // chờ Lv5 (UI sẽ gán Lv3 = null)

                // 4. Transaction
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var request = new F03OTRequest
                {
                    OTCode = otCode,
                    EmployeeCode = creator.EmployeeCode!,
                    CreatedByEmail = creator.Email,
                    DeptCode = model.DeptCode,
                    ScopeType = model.ScopeType,
                    OTDate = model.OTDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    PlannedHours = model.PlannedHours,
                    TotalOTHours = model.PlannedHours,  // cập nhật sau khi GA xác nhận
                    OTTypeCode = model.OTTypeCode,
                    OTReason = model.Reason,
                    RequestStatus = initStatus,
                    // Level 3
                    Level3ApproveEmail = model.Level3ApproveEmail,
                    Level3ApproveCode = model.Level3ApproveCode,
                    Level3ApproveName = model.Level3ApproveName,
                    // Level 5
                    Level5ApproveEmail = model.Level5ApproveEmail,
                    Level5ApproveCode = model.Level5ApproveCode,
                    Level5ApproveName = model.Level5ApproveName,
                    // Level 6
                    Level6ApproveEmail = model.Level6ApproveEmail,
                    Level6ApproveCode = model.Level6ApproveCode,
                    Level6ApproveName = model.Level6ApproveName,
                    // Level 7
                    Level7ApproveEmail = model.Level7ApproveEmail,
                    Level7ApproveCode = model.Level7ApproveCode,
                    Level7ApproveName = model.Level7ApproveName,
                    IsActive = true,
                    CreatedBy = creator.UserId,
                    CreatedAt = DateTime.Now
                };

                _db.F03OTRequests.Add(request);
                await _db.SaveChangesAsync(ct);

                // 5. Thêm danh sách nhân viên OT
                var employees = model.Employees.Select(e => new F03OTEmployee
                {
                    OTRequestId = request.Id,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    DeptCode = e.DeptCode,
                    DeptName = e.DeptName,
                    CvCode = e.CvCode,
                    OTTypeCode = model.OTTypeCode,
                    OTHours = e.OTHours,
                    OTRateMultiplier = OTTypeConst.GetRateMultiplier(model.OTTypeCode),
                    CreatedAt = DateTime.Now
                }).ToList();

                _db.F03OTEmployees.AddRange(employees);
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                Logger.LogInfoIf(Debug, "[OT] Created: {Code} | Id={Id}", otCode, request.Id);

                // 6. Gửi thông báo bất đồng bộ
                _ = FireNotificationAsync(request, creator.FullName ?? creator.EmployeeCode!, ct);

                return ServiceResult<int>.Ok(request.Id, $"Tạo đơn OT thành công. Mã: {otCode}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] Create ERROR");
                return ServiceResult<int>.Fail("Lỗi hệ thống khi tạo đơn OT.");
            }
        }

        // ── APPROVE ────────────────────────────────────────────────────────────
        public async Task<ServiceResult> ApproveAsync(
            List<int> ids,
            int level,
            CurrentUser approver,
            string? comment,
            CancellationToken ct = default)
            => await ProcessBatchAsync(ids, level, approver, comment, isApprove: true, ct);

        // ── REJECT ─────────────────────────────────────────────────────────────
        public async Task<ServiceResult> RejectAsync(
            List<int> ids,
            int level,
            CurrentUser approver,
            string? comment,
            CancellationToken ct = default)
            => await ProcessBatchAsync(ids, level, approver, comment, isApprove: false, ct);

        // ── CANCEL ─────────────────────────────────────────────────────────────
        public async Task<ServiceResult> CancelAsync(
            int id,
            string reason,
            CurrentUser actor,
            CancellationToken ct = default)
        {
            try
            {
                var req = await _db.F03OTRequests
                    .FirstOrDefaultAsync(x => x.Id == id, ct);

                if (req == null) return ServiceResult.Fail("Không tìm thấy đơn OT.");

                bool isOwner = req.EmployeeCode == actor.EmployeeCode;
                bool isAdmin = actor.IsAdmin() || actor.IsSuperAdmin();

                if (!isOwner && !isAdmin)
                    return ServiceResult.Fail("Không có quyền hủy đơn này.");

                if (req.RequestStatus == OTStatus.Approved && !isAdmin)
                    return ServiceResult.Fail("Đơn đã duyệt hoàn tất, không thể hủy.");

                req.RequestStatus = OTStatus.Cancelled;
                req.ModifiedBy = actor.UserId;
                req.ModifiedAt = DateTime.Now;
                // Ghi chú lý do hủy vào comment của bước hiện tại
                req.Level3Comment = $"[Cancelled by {(isOwner ? "User" : "Admin")} {actor.FullName}]: {reason}";

                await _db.SaveChangesAsync(ct);

                // Thông báo cho người tạo (nếu admin hủy thay)
                if (isAdmin && !isOwner)
                {
                    var creatorEmail = await _db.VF03employees
                        .AsNoTracking()
                        .Where(e => e.EmployeeCode == req.EmployeeCode)
                        .Select(e => e.EmailAddress)
                        .FirstOrDefaultAsync(ct);

                    if (!string.IsNullOrEmpty(creatorEmail))
                        await _notification.SendStatusChangedAsync(
                            creatorEmail, req.EmployeeCode, OTStatus.Cancelled, id, ct);
                }

                Logger.LogInfoIf(Debug, "[OT] Cancelled: {Id}", id);
                return ServiceResult.Ok("Đã hủy đơn OT thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] Cancel ERROR: {Id}", id);
                return ServiceResult.Fail("Lỗi hệ thống khi hủy đơn OT.");
            }
        }

        // ── GET DETAILS ────────────────────────────────────────────────────────
        public async Task<ServiceResult<OTRequestViewModel>> GetDetailsAsync(
            int id,
            CancellationToken ct = default)
        {
            var raw = await _db.VF03OTRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (raw == null)
                return ServiceResult<OTRequestViewModel>.Fail("Không tìm thấy đơn OT.");

            var vm = (_query as OTQueryService)!.MapToViewModel(raw);

            // Lấy thêm danh sách nhân viên trong đơn
            vm.Employees = await _db.F03OTEmployees
                .AsNoTracking()
                .Where(e => e.OTRequestId == id)
                .Select(e => new OTEmployeeModel
                {
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    DeptCode = e.DeptCode,
                    DeptName = e.DeptName,
                    CvCode = e.CvCode,
                    OTHours = e.OTHours,
                    OTRateMultiplier = e.OTRateMultiplier,
                    ValidationStatus = e.ValidationStatus,
                    ValidationMessage = e.ValidationMessage,
                    Note = e.Note
                })
                .ToListAsync(ct);

            return ServiceResult<OTRequestViewModel>.Ok(vm);
        }

        // ── GET BALANCE ────────────────────────────────────────────────────────
        public async Task<ServiceResult<OTBalanceDto>> GetBalanceAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct = default)
        {
            try
            {
                var combined = await (_query as OTQueryService)!
                    .GetBalanceInternalAsync(employeeCode, year, month, ct);

                // Phương thức internal — tham chiếu trực tiếp để tránh round-trip
                // Nếu muốn interface sạch hơn, có thể expose qua IOTQueryService
                return ServiceResult<OTBalanceDto>.Ok(combined);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] GetBalance ERROR: {Emp}", employeeCode);
                return ServiceResult<OTBalanceDto>.Fail("Lỗi lấy số dư giờ OT.");
            }
        }

        // ── BATCH APPROVE / REJECT ─────────────────────────────────────────────
        private async Task<ServiceResult> ProcessBatchAsync(
            List<int> ids,
            int level,
            CurrentUser actor,
            string? comment,
            bool isApprove,
            CancellationToken ct)
        {
            if (ids == null || ids.Count == 0)
                return ServiceResult.Fail("Không có đơn nào được chọn.");

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var requests = await _db.F03OTRequests
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync(ct);

                bool isAdmin = actor.IsAdmin() || actor.IsSuperAdmin();
                int success = 0;
                var now = DateTime.Now;

                foreach (var req in requests)
                {
                    // Bỏ qua đơn đã kết thúc luồng
                    if (req.RequestStatus is OTStatus.Approved
                                          or OTStatus.Rejected
                                          or OTStatus.Cancelled)
                        continue;

                    // Kiểm tra quyền approver (trừ Admin)
                    if (!isAdmin)
                    {
                        string? expectedCode = GetApproverCode(req, level);
                        if (actor.EmployeeCode != expectedCode) continue;
                    }

                    string finalComment = isAdmin
                        ? $"[Admin {actor.FullName} duyệt thay lv{level}]: {comment}"
                        : comment ?? string.Empty;

                    if (isApprove)
                    {
                        ApplyApprove(req, level, finalComment, now);
                        UpdateOverallStatus(req);

                        // Gửi thông báo cho approver cấp tiếp theo
                        string? nextEmail = GetNextApproverEmail(req);
                        if (!string.IsNullOrEmpty(nextEmail))
                            _ = _notification.SendApprovalRequestAsync(
                                nextEmail, GetNextApproverName(req, level) ?? "",
                                req, actor.FullName ?? "", ct);

                        // Nếu Approved hoàn tất → notify người tạo
                        if (req.RequestStatus == OTStatus.Approved)
                        {
                            var creatorEmail = await ResolveCreatorEmailAsync(req, ct);
                            if (!string.IsNullOrEmpty(creatorEmail))
                                _ = _notification.SendStatusChangedAsync(
                                    creatorEmail, req.EmployeeCode,
                                    OTStatus.Approved, req.Id, ct);
                        }
                    }
                    else
                    {
                        ApplyReject(req, level, finalComment, now);
                        req.RequestStatus = OTStatus.Rejected;

                        var creatorEmail = await ResolveCreatorEmailAsync(req, ct);
                        if (!string.IsNullOrEmpty(creatorEmail))
                            _ = _notification.SendStatusChangedAsync(
                                creatorEmail, req.EmployeeCode,
                                OTStatus.Rejected, req.Id, ct);
                    }

                    req.ModifiedBy = actor.UserId;
                    req.ModifiedAt = now;
                    success++;
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                string action = isApprove ? "duyệt" : "từ chối";
                Logger.LogInfoIf(Debug, "[OT] Batch {Action}: {Ok}/{Total}", action, success, requests.Count);

                return success > 0
                    ? ServiceResult.Ok($"Đã {action} {success}/{requests.Count} đơn OT.")
                    : ServiceResult.Fail("Không có đơn nào đủ điều kiện xử lý.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] Batch ERROR level={Level}", level);
                return ServiceResult.Fail("Lỗi hệ thống khi xử lý đơn OT.");
            }
        }

        // ── STATIC HELPERS ──────────────────────────────────────────────────────
        private static void ApplyApprove(F03OTRequest req, int level, string comment, DateTime now)
        {
            switch (level)
            {
                case 3: req.Level3IsApprove = true; req.Level3ApproveTime = now; req.Level3Comment = comment; break;
                case 5: req.Level5IsApprove = true; req.Level5ApproveTime = now; req.Level5Comment = comment; break;
                case 6: req.Level6IsApprove = true; req.Level6ApproveTime = now; req.Level6Comment = comment; break;
                case 7: req.Level7IsApprove = true; req.Level7ApproveTime = now; req.Level7Comment = comment; break;
            }
        }

        private static void ApplyReject(F03OTRequest req, int level, string comment, DateTime now)
        {
            switch (level)
            {
                case 3: req.Level3IsApprove = false; req.Level3ApproveTime = now; req.Level3Comment = comment; break;
                case 5: req.Level5IsApprove = false; req.Level5ApproveTime = now; req.Level5Comment = comment; break;
                case 6: req.Level6IsApprove = false; req.Level6ApproveTime = now; req.Level6Comment = comment; break;
                case 7: req.Level7IsApprove = false; req.Level7ApproveTime = now; req.Level7Comment = comment; break;
            }
        }

        /// <summary>
        /// Cập nhật RequestStatus theo thứ tự flow:
        ///   Lv3 ✓ → ApprovedLv3 → Lv5 ✓ → ApprovedLv5 → Lv6 ✓ → ApprovedLv6 → Lv7 ✓ → Approved
        ///   Nếu bỏ qua Lv3 (văn phòng) thì bắt đầu từ Lv5.
        /// </summary>
        private static void UpdateOverallStatus(F03OTRequest req)
        {
            // Nếu có Lv7 và đã duyệt → Approved hoàn tất
            if (!string.IsNullOrEmpty(req.Level7ApproveEmail) && req.Level7IsApprove == true)
            { req.RequestStatus = OTStatus.Approved; return; }

            // Lv7 không có → Approved khi Lv6 đã duyệt (văn phòng không cần GM)
            if (string.IsNullOrEmpty(req.Level7ApproveEmail) && req.Level6IsApprove == true)
            { req.RequestStatus = OTStatus.Approved; return; }

            if (req.Level6IsApprove == true) { req.RequestStatus = OTStatus.ApprovedLv6; return; }
            if (req.Level5IsApprove == true) { req.RequestStatus = OTStatus.ApprovedLv5; return; }
            if (req.Level3IsApprove == true) { req.RequestStatus = OTStatus.ApprovedLv3; return; }

            req.RequestStatus = OTStatus.Pending;
        }

        private static string? GetApproverCode(F03OTRequest req, int level) => level switch
        {
            3 => req.Level3ApproveCode,
            5 => req.Level5ApproveCode,
            6 => req.Level6ApproveCode,
            7 => req.Level7ApproveCode,
            _ => null
        };

        private static string? GetNextApproverEmail(F03OTRequest req) => req.RequestStatus switch
        {
            OTStatus.ApprovedLv3 => req.Level5ApproveEmail,
            OTStatus.ApprovedLv5 => req.Level6ApproveEmail,
            OTStatus.ApprovedLv6 => req.Level7ApproveEmail,
            _ => null
        };

        private static string? GetNextApproverName(F03OTRequest req, int justDoneLevel) =>
            justDoneLevel switch
            {
                3 => req.Level5ApproveName,
                5 => req.Level6ApproveName,
                6 => req.Level7ApproveName,
                _ => null
            };

        private async Task<string?> ResolveCreatorEmailAsync(F03OTRequest req, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(req.CreatedByEmail)) return req.CreatedByEmail;

            return await _db.VF03employees
                .AsNoTracking()
                .Where(e => e.EmployeeCode == req.EmployeeCode)
                .Select(e => e.EmailAddress)
                .FirstOrDefaultAsync(ct);
        }

        private async Task FireNotificationAsync(
            F03OTRequest request,
            string creatorName,
            CancellationToken ct)
        {
            try
            {
                // Approver đầu tiên trong flow
                string? firstEmail = !string.IsNullOrEmpty(request.Level3ApproveEmail)
                    ? request.Level3ApproveEmail
                    : request.Level5ApproveEmail;

                string? firstName = !string.IsNullOrEmpty(request.Level3ApproveName)
                    ? request.Level3ApproveName
                    : request.Level5ApproveName;

                if (!string.IsNullOrEmpty(firstEmail))
                    await _notification.SendApprovalRequestAsync(
                        firstEmail, firstName ?? "", request, creatorName, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] FireNotification ERROR: {Code}", request.OTCode);
            }
        }
    }
}