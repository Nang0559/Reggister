using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FVN_REGISTER.API.Services.Leaves
{
    namespace FVN_REGISTER.API.Services.Leaves
    {
        public class LeaveEscalationService
            : BaseService<LeaveEscalationService>, ILeaveEscalationService
        {
            private readonly FVNWEBAPPContext _db;
            private readonly IEscalationRuleService _rule;
            private readonly IWorkingDayService _workingDay;
            private readonly IEmailService _email;

            public LeaveEscalationService(
                FVNWEBAPPContext db,
                IEscalationRuleService rule,
                IWorkingDayService workingDay,
                IEmailService email,
                ILogger<LeaveEscalationService> logger,
                IOptionsMonitor<AuthDebugOptions> options)
                : base(logger, options)
            {
                _db = db;
                _rule = rule;
                _workingDay = workingDay;
                _email = email;
            }

            public async Task ProcessAutoEscalationAsync(CancellationToken ct = default)
            {
                try
                {
                    Logger.LogDebugIf(Debug, "[ESC] Start escalation job");

                    var leaves = await _db.F03leaveDays
                        .Where(x => x.IsActive == true &&
                                    LeaveStatus.ActiveStatuses.Contains(x.RequestStatus))
                        .ToListAsync(ct);

                    if (!leaves.Any())
                    {
                        Logger.LogDebugIf(Debug, "[ESC] No active leaves");
                        return;
                    }

                    var empCodes = leaves
                        .Select(x => x.EmployeeCode)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();

                    var employeeDict = await _db.VF03employees
                        .Where(x => empCodes.Contains(x.EmployeeCode))
                        .ToDictionaryAsync(x => x.EmployeeCode, x => x, ct);

                    foreach (var leave in leaves)
                    {
                        ct.ThrowIfCancellationRequested();

                        employeeDict.TryGetValue(leave.EmployeeCode ?? "", out var emp);
                        var deptCode = emp?.DeptCode ?? "ALL";

                        bool changed = false;

                        // ── LEVEL 1 ──
                        // Chỉ xử lý khi Level1 chưa có quyết định
                        if (leave.Level1IsApprove == null)
                        {
                            changed |= await ProcessLevelAsync(
                                leave, 1, leave.RegisterDate,
                                approverEmail: leave.Level1ApproveEmail,
                                deptCode: deptCode,
                                setApprove: v => leave.Level1IsApprove = v,
                                setTime: t => leave.Level1ApproveTime = t,
                                setComment: c => leave.Level1Comment = c,
                                isFinal: string.IsNullOrEmpty(leave.Level2ApproveEmail) &&
                                         string.IsNullOrEmpty(leave.Level3ApproveEmail),
                                ct: ct);
                        }

                        // ── LEVEL 2 ──
                        // Chỉ xử lý khi Level1 đã approve VÀ Level2 chưa có quyết định
                        if (leave.Level1IsApprove == true && leave.Level2IsApprove == null)
                        {
                            changed |= await ProcessLevelAsync(
                                leave, 2,
                                baseTime: leave.Level1ApproveTime ?? leave.RegisterDate,
                                approverEmail: leave.Level2ApproveEmail,
                                deptCode: deptCode,
                                setApprove: v => leave.Level2IsApprove = v,
                                setTime: t => leave.Level2ApproveTime = t,
                                setComment: c => leave.Level2Comment = c,
                                isFinal: string.IsNullOrEmpty(leave.Level3ApproveEmail),
                                ct: ct);
                        }

                        // ── LEVEL 3 ──
                        // Chỉ xử lý khi Level2 đã approve VÀ Level3 chưa có quyết định
                        if (leave.Level2IsApprove == true && leave.Level3IsApprove == null &&
                            !string.IsNullOrEmpty(leave.Level3ApproveEmail))
                        {
                            changed |= await ProcessLevelAsync(
                                leave, 3,
                                baseTime: leave.Level2ApproveTime ?? leave.RegisterDate,
                                approverEmail: leave.Level3ApproveEmail,
                                deptCode: deptCode,
                                setApprove: v => leave.Level3IsApprove = v,
                                setTime: t => leave.Level3ApproveTime = t,
                                setComment: c => leave.Level3Comment = c,
                                isFinal: true,
                                ct: ct);
                        }

                        // Cập nhật RequestStatus nếu có thay đổi
                        if (changed)
                            UpdateOverallStatus(leave);
                    }

                    await _db.SaveChangesAsync(ct);

                    Logger.LogInfoIf(Debug, "[ESC] Completed. Total leaves: {Count}", leaves.Count);
                }
                catch (OperationCanceledException)
                {
                    Logger.LogWarnIf(Debug, "[ESC] Cancelled by token");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[ESC] Error in escalation job");
                    throw;
                }
            }

            // ✅ Trả về bool để caller biết có thay đổi không
            private async Task<bool> ProcessLevelAsync(
                F03leaveDay leave,
                int level,
                DateTime? baseTime,
                string? approverEmail,
                string deptCode,
                Action<bool?> setApprove,
                Action<DateTime?> setTime,
                Action<string?> setComment,
                bool isFinal,
                CancellationToken ct)
            {
                // Guard: không có baseTime hoặc không có approver → bỏ qua
                if (!baseTime.HasValue || string.IsNullOrEmpty(approverEmail))
                    return false;

                int timeout = await _rule.GetTimeoutDaysAsync(leave, level, deptCode, ct);
                int workingDays = await _workingDay.GetWorkingDaysAsync(baseTime.Value, DateTime.Now);

                Logger.LogDebugIf(Debug,
                    "[ESC] LeaveId={LeaveId} Level={Level} WorkingDays={Days}/{Timeout}",
                    leave.Id, level, workingDays, timeout);

                if (workingDays < timeout)
                    return false;

                // Timeout đã vượt quá → escalate (tự động từ chối cấp này)
                setApprove(false);
                setTime(DateTime.Now);
                setComment($"Auto escalation level {level} (timeout {timeout} ngày làm việc)");

                _db.EscalationLogs.Add(new EscalationLog
                {
                    LeaveId = leave.Id,
                    Level = level,
                    Action = isFinal ? "AutoReject" : "Escalated",
                    CreatedAt = DateTime.Now
                });

                // Gửi email thông báo cho approver đã timeout
                await _email.QueueEmail(
                    approverEmail,
                    $"LEAVE_ESCALATED_LV{level}",
                    new
                    {
                        leave.EmployeeCode,
                        Level = level,
                        TimeoutDays = timeout
                    },
                    ct);

                Logger.LogWarnIf(Debug,
                    "[ESC] LeaveId={LeaveId} Level={Level} escalated | IsFinal={IsFinal}",
                    leave.Id, level, isFinal);

                return true;
            }

            // Giống logic trong LeaveService để đồng bộ trạng thái
            private static void UpdateOverallStatus(F03leaveDay leave)
            {
                // Bất kỳ level nào bị từ chối (false) → Rejected
                if (leave.Level1IsApprove == false ||
                    leave.Level2IsApprove == false ||
                    leave.Level3IsApprove == false)
                {
                    leave.RequestStatus = LeaveStatus.Rejected;
                    return;
                }

                // Level 3 approve xong → hoàn tất
                if (!string.IsNullOrEmpty(leave.Level3ApproveEmail) && leave.Level3IsApprove == true)
                {
                    leave.RequestStatus = LeaveStatus.Approved;
                    return;
                }

                // Level 2 approve xong
                if (leave.Level2IsApprove == true)
                {
                    leave.RequestStatus = string.IsNullOrEmpty(leave.Level3ApproveEmail)
                        ? LeaveStatus.Approved
                        : LeaveStatus.ApprovedLv2;
                    return;
                }

                // Level 1 approve xong
                if (leave.Level1IsApprove == true)
                {
                    leave.RequestStatus = (string.IsNullOrEmpty(leave.Level2ApproveEmail) &&
                                           string.IsNullOrEmpty(leave.Level3ApproveEmail))
                        ? LeaveStatus.Approved
                        : LeaveStatus.ApprovedLv1;
                    return;
                }

                leave.RequestStatus = LeaveStatus.Pending;
            }
        }
    }
}
