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

namespace FVN_REGISTER.API.Services.OT
{
    public class OTService : BaseService<OTService>, IOTService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IOTValidator _validator;
        private readonly IOTNotificationService _notification;
        private readonly IOTQueryService _query;

        public OTService(
            FVNWEBAPPContext db,
            IOTValidator validator,
            IOTNotificationService notification,
            IOTQueryService query,
            ILogger<OTService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _validator = validator;
            _notification = notification;
            _query = query;
        }

        // ════════════════════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult<int>> CreateAsync(
            CreateOTRequestModel model,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[OT] Create start: {User}", user.EmployeeCode);

                // 1. Validate
                var valResult = await _validator.ValidateCreateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[OT] Validation failed: {Msg}", valResult.Message);
                    return ServiceResult<int>.Fail(valResult.Message!);
                }

                // 2. Tính giờ OT thực tế (server-side — không tin client)
                var plannedHours = OTValidator.CalcHours(model.StartTime, model.EndTime);

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // 3. Tạo mã OT duy nhất
                var otCode = GenerateOTCode(model.OTDate);

                // 4. Lưu F03OTRequest
                var otRequest = new F03OTRequest
                {
                    OTCode = otCode,
                    EmployeeCode = user.EmployeeCode!,
                    CreatedByEmail = user.Email,
                    DeptCode = model.DeptCode,
                    ScopeType = model.ScopeType,
                    OTDate = model.OTDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    PlannedHours = plannedHours,
                    TotalOTHours = plannedHours,
                    OTTypeCode = model.OTTypeCode,
                    OTReason = model.Reason,
                    RequestStatus = OTStatus.Pending,
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
                    CreatedBy = user.UserId,
                    CreatedAt = DateTime.Now
                };

                _db.F03OTRequests.Add(otRequest);
                await _db.SaveChangesAsync(ct);

                // 5. Lưu danh sách nhân viên F03OTEmployee
                var otRate = OTTypeConst.GetRateMultiplier(model.OTTypeCode);
                var employees = model.Employees.Select(e => new F03OTEmployee
                {
                    OTRequestId = otRequest.Id,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    DeptCode = e.DeptCode ?? model.DeptCode,
                    DeptName = e.DeptName,
                    CvCode = e.CvCode,
                    OTTypeCode = model.OTTypeCode,
                    OTHours = plannedHours,
                    OTRateMultiplier = otRate,
                    CreatedAt = DateTime.Now
                }).ToList();

                _db.F03OTEmployees.AddRange(employees);
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                Logger.LogInfoIf(Debug,
                    "[OT] Created OTCode={Code} Id={Id}",
                    otCode, otRequest.Id);

                // 6. Gửi notification bất đồng bộ (fire-and-forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Công nhân bắt đầu từ Level 3, VP từ Level 5
                        var firstLevel = model.HasWorker ? 3 : 5;
                        var firstEmail = model.HasWorker
                            ? model.Level3ApproveEmail
                            : model.Level5ApproveEmail;
                        var firstApproverName = model.HasWorker
                            ? model.Level3ApproveName
                            : model.Level5ApproveName;

                        if (!string.IsNullOrWhiteSpace(firstEmail))
                        {
                            await _notification.SendApprovalRequestAsync(
                                firstEmail!,
                                firstApproverName ?? "",
                                otRequest,
                                user.FullName ?? user.EmployeeCode ?? "",
                                firstLevel,
                                CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "[OT] Notification failed for OT {Id}", otRequest.Id);
                    }
                }, CancellationToken.None);

                return ServiceResult<int>.Ok(otRequest.Id, "Tạo đơn OT thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] CreateAsync error");
                return ServiceResult<int>.Fail("Lỗi hệ thống khi tạo đơn OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // APPROVE
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult> ApproveAsync(
            List<int> otRequestIds,
            int level,
            CurrentUser user,
            string? comment,
            CancellationToken ct = default)
        {
            if (otRequestIds == null || otRequestIds.Count == 0)
                return ServiceResult.Fail("Không có đơn nào được chọn.");

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var requests = await _db.F03OTRequests
                    .Where(x => otRequestIds.Contains(x.Id) && x.IsActive == true)
                    .ToListAsync(ct);

                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();
                int success = 0;
                var now = DateTime.Now;

                foreach (var req in requests)
                {
                    // Bỏ qua các đơn đã kết thúc
                    if (req.RequestStatus is OTStatus.Approved
                                          or OTStatus.Rejected
                                          or OTStatus.Cancelled)
                        continue;

                    // Kiểm tra người duyệt có đúng email không (admin bypass)
                    var approverEmail = GetApproverEmail(req, level);
                    if (!isAdmin && !string.Equals(user.Email, approverEmail,
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Kiểm tra thứ tự duyệt hợp lệ
                    if (!IsReadyForLevel(req, level))
                        continue;

                    ApplyApproveData(req, level, user, comment, isAdmin, now);
                    UpdateOverallStatus(req);
                    success++;

                    // Gửi notification bước tiếp theo (fire-and-forget)
                    var finalReq = req;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendNextLevelOrCompletionAsync(finalReq, user, ct);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "[OT] Post-approve notification failed for {Id}", finalReq.Id);
                        }
                    }, CancellationToken.None);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                Logger.LogInfoIf(Debug, "[OT] Approved {Success}/{Total}", success, requests.Count);

                return success > 0
                    ? ServiceResult.Ok($"Đã duyệt {success}/{requests.Count} đơn.")
                    : ServiceResult.Fail("Không có đơn nào đủ điều kiện duyệt.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] ApproveAsync error");
                return ServiceResult.Fail("Lỗi hệ thống khi phê duyệt đơn OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // REJECT
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult> RejectAsync(
            List<int> otRequestIds,
            int level,
            CurrentUser user,
            string comment,
            CancellationToken ct = default)
        {
            if (otRequestIds == null || otRequestIds.Count == 0)
                return ServiceResult.Fail("Không có đơn nào được chọn.");

            if (string.IsNullOrWhiteSpace(comment))
                return ServiceResult.Fail("Lý do từ chối không được để trống.");

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var requests = await _db.F03OTRequests
                    .Where(x => otRequestIds.Contains(x.Id) && x.IsActive == true)
                    .ToListAsync(ct);

                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();
                int success = 0;
                var now = DateTime.Now;

                foreach (var req in requests)
                {
                    if (req.RequestStatus is OTStatus.Approved
                                          or OTStatus.Rejected
                                          or OTStatus.Cancelled)
                        continue;

                    var approverEmail = GetApproverEmail(req, level);
                    if (!isAdmin && !string.Equals(user.Email, approverEmail,
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    ApplyRejectData(req, level, user, comment, isAdmin, now);
                    req.RequestStatus = OTStatus.Rejected;
                    success++;

                    // Thông báo kết quả cho người tạo
                    var finalReq = req;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var creator = await _db.VF03employees
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.EmployeeCode == finalReq.EmployeeCode,
                                    CancellationToken.None);

                            if (!string.IsNullOrWhiteSpace(finalReq.CreatedByEmail))
                            {
                                await _notification.SendStatusChangedAsync(
                                    finalReq.CreatedByEmail!,
                                    creator?.EmployeeName ?? finalReq.EmployeeCode,
                                    OTStatus.Rejected,
                                    finalReq,
                                    CancellationToken.None);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "[OT] Reject notification failed for {Id}", finalReq.Id);
                        }
                    }, CancellationToken.None);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                Logger.LogInfoIf(Debug, "[OT] Rejected {Success}/{Total}", success, requests.Count);

                return success > 0
                    ? ServiceResult.Ok($"Đã từ chối {success}/{requests.Count} đơn.")
                    : ServiceResult.Fail("Không có đơn nào đủ điều kiện từ chối.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] RejectAsync error");
                return ServiceResult.Fail("Lỗi hệ thống khi từ chối đơn OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // CANCEL
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult> CancelAsync(
            int otRequestId,
            string reason,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                var req = await _db.F03OTRequests
                    .FirstOrDefaultAsync(x => x.Id == otRequestId, ct);

                if (req == null)
                    return ServiceResult.Fail("Không tìm thấy đơn OT.");

                bool isOwner = string.Equals(req.EmployeeCode, user.EmployeeCode,
                                   StringComparison.OrdinalIgnoreCase);
                bool isAdmin = user.IsAdmin() || user.IsSuperAdmin();

                if (!isOwner && !isAdmin)
                    return ServiceResult.Fail("Không có quyền hủy đơn này.");

                if (req.RequestStatus == OTStatus.Approved && !isAdmin)
                    return ServiceResult.Fail("Đơn đã duyệt hoàn tất, không thể hủy.");

                req.RequestStatus = OTStatus.Cancelled;
                req.IsActive = false;
                req.ModifiedBy = user.UserId;
                req.ModifiedAt = DateTime.Now;
                // Ghi lý do hủy vào comment của bước đang chờ
                var cancelNote = $"[Cancelled by {(isOwner ? "Owner" : "Admin")}]: {reason}";
                if (req.Level3IsApprove == null) req.Level3Comment = cancelNote;
                else if (req.Level5IsApprove == null) req.Level5Comment = cancelNote;
                else if (req.Level6IsApprove == null) req.Level6Comment = cancelNote;
                else req.Level7Comment = cancelNote;

                await _db.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[OT] Cancelled Id={Id} by {User}", otRequestId, user.EmployeeCode);

                return ServiceResult.Ok("Đã hủy đơn OT thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] CancelAsync error Id={Id}", otRequestId);
                return ServiceResult.Fail("Lỗi hệ thống khi hủy đơn OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // GET DETAILS
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult<OTRequestViewModel>> GetDetailsAsync(
            int otRequestId,
            CancellationToken ct = default)
        {
            try
            {
                var raw = await _db.VF03OTRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == otRequestId, ct);

                if (raw == null)
                    return ServiceResult<OTRequestViewModel>.Fail("Không tìm thấy đơn OT.");

                var vm = (_query as OTQueryService)!.MapToViewModel(raw);

                // Gắn thêm danh sách nhân viên trong đơn
                var employees = await _db.F03OTEmployees
                    .AsNoTracking()
                    .Where(e => e.OTRequestId == otRequestId)
                    .Select(e => new OTEmployeeModel
                    {
                        EmployeeCode = e.EmployeeCode,
                        EmployeeName = e.EmployeeName,
                        DeptCode = e.DeptCode,
                        DeptName = e.DeptName,
                        CvCode = e.CvCode,
                        OTHours = e.OTHours,
                        OTTypeCode = e.OTTypeCode,
                        ActualHours = e.ActualHours,
                        Note = e.Note
                    })
                    .ToListAsync(ct);

                vm.Employees = employees;
                return ServiceResult<OTRequestViewModel>.Ok(vm);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] GetDetailsAsync error Id={Id}", otRequestId);
                return ServiceResult<OTRequestViewModel>.Fail("Lỗi hệ thống khi lấy chi tiết đơn OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // GET BALANCE
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult<OTBalanceDto>> GetBalanceAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct = default)
        {
            try
            {
                var balance = await _query.GetBalanceAsync(employeeCode, year, month, ct);
                return ServiceResult<OTBalanceDto>.Ok(balance);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] GetBalanceAsync error Emp={Emp}", employeeCode);
                return ServiceResult<OTBalanceDto>.Fail("Lỗi hệ thống khi lấy số dư giờ OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // VALIDATE HOURS (delegate to query service)
        // ════════════════════════════════════════════════════════════════════
        public async Task<ServiceResult<OTValidationResultDto>> ValidateHoursAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _query.ValidateHoursAsync(model, ct);
                return ServiceResult<OTValidationResultDto>.Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT] ValidateHoursAsync error");
                return ServiceResult<OTValidationResultDto>.Fail("Lỗi hệ thống khi validate giờ OT.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Sinh mã OT dạng OT-YYYYMMDD-XXXX (random 4 ký tự cuối)</summary>
        private static string GenerateOTCode(DateTime otDate)
        {
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"OT-{otDate:yyyyMMdd}-{suffix}";
        }

        /// <summary>Lấy email approver theo level của một F03OTRequest</summary>
        private static string? GetApproverEmail(F03OTRequest req, int level) => level switch
        {
            3 => req.Level3ApproveEmail,
            5 => req.Level5ApproveEmail,
            6 => req.Level6ApproveEmail,
            7 => req.Level7ApproveEmail,
            _ => null
        };

        /// <summary>
        /// Kiểm tra đơn có sẵn sàng để duyệt ở level chỉ định không.
        /// Logic: mỗi level phải chờ level trước đó approve xong.
        /// Level 3 không có điều kiện tiên quyết (chỉ cần Pending).
        /// Level 5 yêu cầu Level3 đã duyệt (hoặc Level3 không được set — VP không cần Lv3).
        /// Level 6 yêu cầu Level5 đã duyệt.
        /// Level 7 yêu cầu Level6 đã duyệt.
        /// </summary>
        private static bool IsReadyForLevel(F03OTRequest req, int level) => level switch
        {
            3 => req.Level3IsApprove == null,
            5 => (req.Level3IsApprove == true || string.IsNullOrEmpty(req.Level3ApproveEmail))
                 && req.Level5IsApprove == null,
            6 => req.Level5IsApprove == true && req.Level6IsApprove == null,
            7 => req.Level6IsApprove == true && req.Level7IsApprove == null,
            _ => false
        };

        /// <summary>Ghi dữ liệu Approve vào đúng level</summary>
        private static void ApplyApproveData(
            F03OTRequest req,
            int level,
            CurrentUser user,
            string? comment,
            bool isAdmin,
            DateTime now)
        {
            var finalComment = isAdmin
                ? $"[Admin {user.FullName} duyệt thay]: {comment}"
                : comment;

            switch (level)
            {
                case 3:
                    req.Level3IsApprove = true;
                    req.Level3ApproveTime = now;
                    req.Level3Comment = finalComment;
                    break;
                case 5:
                    req.Level5IsApprove = true;
                    req.Level5ApproveTime = now;
                    req.Level5Comment = finalComment;
                    break;
                case 6:
                    req.Level6IsApprove = true;
                    req.Level6ApproveTime = now;
                    req.Level6Comment = finalComment;
                    break;
                case 7:
                    req.Level7IsApprove = true;
                    req.Level7ApproveTime = now;
                    req.Level7Comment = finalComment;
                    break;
            }
        }

        /// <summary>Ghi dữ liệu Reject vào đúng level</summary>
        private static void ApplyRejectData(
            F03OTRequest req,
            int level,
            CurrentUser user,
            string comment,
            bool isAdmin,
            DateTime now)
        {
            var finalComment = isAdmin
                ? $"[Admin {user.FullName} từ chối]: {comment}"
                : comment;

            switch (level)
            {
                case 3:
                    req.Level3IsApprove = false;
                    req.Level3ApproveTime = now;
                    req.Level3Comment = finalComment;
                    break;
                case 5:
                    req.Level5IsApprove = false;
                    req.Level5ApproveTime = now;
                    req.Level5Comment = finalComment;
                    break;
                case 6:
                    req.Level6IsApprove = false;
                    req.Level6ApproveTime = now;
                    req.Level6Comment = finalComment;
                    break;
                case 7:
                    req.Level7IsApprove = false;
                    req.Level7ApproveTime = now;
                    req.Level7Comment = finalComment;
                    break;
            }
        }

        /// <summary>
        /// Cập nhật RequestStatus tổng thể sau khi một level approve.
        /// Flow: Pending → ApprovedLv3 → ApprovedLv5 → ApprovedLv6 → Approved
        /// VP (không có Level3): Pending → ApprovedLv5 → ApprovedLv6 → Approved
        /// </summary>
        private static void UpdateOverallStatus(F03OTRequest req)
        {
            bool hasLv3 = !string.IsNullOrEmpty(req.Level3ApproveEmail);
            bool hasLv7 = !string.IsNullOrEmpty(req.Level7ApproveEmail);

            // Nếu Level7 vừa duyệt → hoàn tất
            if (hasLv7 && req.Level7IsApprove == true)
            {
                req.RequestStatus = OTStatus.Approved;
                return;
            }

            // Level6 vừa duyệt
            if (req.Level6IsApprove == true)
            {
                // Nếu không cần Level7 → hoàn tất
                req.RequestStatus = hasLv7 ? OTStatus.ApprovedLv6 : OTStatus.Approved;
                return;
            }

            // Level5 vừa duyệt
            if (req.Level5IsApprove == true)
            {
                req.RequestStatus = OTStatus.ApprovedLv5;
                return;
            }

            // Level3 vừa duyệt (chỉ dành cho công nhân)
            if (hasLv3 && req.Level3IsApprove == true)
            {
                req.RequestStatus = OTStatus.ApprovedLv3;
                return;
            }

            req.RequestStatus = OTStatus.Pending;
        }

        /// <summary>
        /// Sau khi một level approve, gửi notification tới level tiếp theo.
        /// Nếu đơn đã Approved hoàn tất thì gửi thông báo kết quả cho người tạo.
        /// </summary>
        private async Task SendNextLevelOrCompletionAsync(
            F03OTRequest req,
            CurrentUser approver,
            CancellationToken ct)
        {
            if (req.RequestStatus == OTStatus.Approved)
            {
                // Thông báo hoàn tất cho người tạo
                if (!string.IsNullOrWhiteSpace(req.CreatedByEmail))
                {
                    var creator = await _db.VF03employees
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EmployeeCode == req.EmployeeCode, ct);

                    await _notification.SendStatusChangedAsync(
                        req.CreatedByEmail!,
                        creator?.EmployeeName ?? req.EmployeeCode,
                        OTStatus.Approved,
                        req,
                        ct);
                }
                return;
            }

            // Xác định level + email kế tiếp
            (int nextLevel, string? nextEmail, string? nextName) = req.RequestStatus switch
            {
                OTStatus.ApprovedLv3 => (5, req.Level5ApproveEmail, req.Level5ApproveName),
                OTStatus.ApprovedLv5 => (6, req.Level6ApproveEmail, req.Level6ApproveName),
                OTStatus.ApprovedLv6 => (7, req.Level7ApproveEmail, req.Level7ApproveName),
                _ => (0, null, null)
            };

            if (nextLevel > 0 && !string.IsNullOrWhiteSpace(nextEmail))
            {
                var creator = await _db.VF03employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.EmployeeCode == req.EmployeeCode, ct);

                await _notification.SendApprovalRequestAsync(
                    nextEmail!,
                    nextName ?? "",
                    req,
                    creator?.EmployeeName ?? req.EmployeeCode,
                    nextLevel,
                    ct);
            }
        }
    }
}