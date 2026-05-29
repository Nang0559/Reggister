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

                    await ProcessLevelAsync(leave, 1, leave.RegisterDate,
                        () => leave.Level1IsApprove,
                        v => leave.Level1IsApprove = v,
                        t => leave.Level1ApproveTime = t,
                        c => leave.Level1Comment = c,
                        leave.Level1ApproveEmail,
                        deptCode,
                        ct);

                    await ProcessLevelAsync(leave, 2,
                        leave.Level1ApproveTime ?? leave.RegisterDate,
                        () => leave.Level2IsApprove,
                        v => leave.Level2IsApprove = v,
                        t => leave.Level2ApproveTime = t,
                        c => leave.Level2Comment = c,
                        leave.Level2ApproveEmail,
                        deptCode,
                        ct);

                    await ProcessLevelAsync(leave, 3,
                        leave.Level2ApproveTime ?? leave.RegisterDate,
                        () => leave.Level3IsApprove,
                        v => leave.Level3IsApprove = v,
                        t => leave.Level3ApproveTime = t,
                        c => leave.Level3Comment = c,
                        leave.Level3ApproveEmail,
                        deptCode,
                        ct,
                        isFinal: true);
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

        private async Task ProcessLevelAsync(
            F03leaveDay leave,
            int level,
            DateTime? baseTime,
            Func<bool?> getStatus,
            Action<bool?> setStatus,
            Action<DateTime?> setTime,
            Action<string?> setComment,
            string? email,
            string deptCode,
            CancellationToken ct,
            bool isFinal = false)
        {
            if (getStatus() != null || !baseTime.HasValue)
                return;

            int timeout = await _rule.GetTimeoutDaysAsync(leave, level, deptCode, ct);

            int workingDays = await _workingDay.GetWorkingDaysAsync(
                baseTime.Value,
                DateTime.Now);

            Logger.LogDebugIf(Debug,
                "[ESC] Leave {LeaveId} Level {Level} WorkingDays {Days}/{Timeout}",
                leave.Id, level, workingDays, timeout);

            if (workingDays < timeout)
                return;

            setStatus(false);
            setTime(DateTime.Now);
            setComment($"Auto escalation level {level} (timeout {timeout} days)");

            _db.EscalationLogs.Add(new EscalationLog
            {
                LeaveId = leave.Id,
                Level = level,
                Action = isFinal ? "AutoReject" : "Escalated",
                CreatedAt = DateTime.Now
            });

            if (!string.IsNullOrEmpty(email))
            {
                await _email.QueueEmail(
                    email,
                    $"LEAVE_PENDING_LV{level}",
                    new { leave.EmployeeCode },
                    ct);
            }

            if (isFinal)
            {
                leave.RequestStatus = LeaveStatus.Rejected;

                Logger.LogWarnIf(Debug,
                    "[ESC] Leave {LeaveId} auto-rejected at final level",
                    leave.Id);
            }
        }
    }
}
