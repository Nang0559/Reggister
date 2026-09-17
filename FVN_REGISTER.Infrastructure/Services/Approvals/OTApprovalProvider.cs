using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public class OTApprovalProvider
    : BaseApprovalProvider<OTRequestSubject, OTApprovalProvider>,
      IApprovalProvider<OTRequestSubject>
{
    public override RequestModule RequestType => RequestModule.Overtime;
    private const int SubLeaderStep = 3;
    private const int ChiefStep = 5;
    private const int ManagerStep = 6;
    private const int GmStep = 7;

    protected override IReadOnlyList<(int Level, string LevelName, string RoleName)> LevelDefs { get; } = new[]
    {
        (SubLeaderStep, "Sub-leader/Leader", "SubLeader"),
        (ChiefStep, "Ast. Chief/Chief", "Chief"),
        (ManagerStep, "A.MG/MG", "Manager"),
        (GmStep, "GM", "GM")
    };

    public OTApprovalProvider(
        IUnitOfWork uow,
        IEmailService email,
        IApprovalNotificationService notification,
        IEmployeeUserResolver userResolver,
        ILogger<OTApprovalProvider> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(uow, email, notification, userResolver, logger, options) { }

    protected override bool? ResolveRequired(int level, ApprovalBuildContext ctx) => level switch
    {
        SubLeaderStep => true,
        ChiefStep => true,
        ManagerStep => true,
        GmStep => true,
        _ => null
    };

    public override async Task<OTRequestSubject?> GetSubjectAsync(int requestId, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03OTRequest>().Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);
        if (entity == null) return null;
        var info = await GetEmployeeInfoAsync(entity.EmployeeCode, ct);
        return OTRequestSubject.From(entity, info?.EmployeeName, info?.PositionCode);
    }

    public override async Task<List<OTRequestSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct)
    {
        var entities = await _uow.Repository<F03OTRequest>().Query().AsNoTracking()
            .Where(x => requestIds.Contains(x.Id) && x.IsActive == true).ToListAsync(ct);
        var empCodes = entities.Select(e => e.EmployeeCode).Distinct().ToList();
        var empMap = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(e => empCodes.Contains(e.EmployeeCode))
            .Select(e => new { e.EmployeeCode, e.EmployeeName, e.PositionCode })
            .ToDictionaryAsync(e => e.EmployeeCode, e => e, ct);
        return entities.Select(x =>
        {
            empMap.TryGetValue(x.EmployeeCode, out var info);
            return OTRequestSubject.From(x, info?.EmployeeName, info?.PositionCode);
        }).ToList();
    }

    public override async Task ApplyOverallStatusAsync(
        int requestId, IReadOnlyList<ApprovalStepCalculatedDto> allSteps, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03OTRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == requestId, ct);
        if (entity == null) return;

        entity.RequestStatus = ComputeOverallStatus(allSteps);
        entity.ModifiedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
    }

    private static ApprovalStatus ComputeOverallStatus(IReadOnlyList<ApprovalStepCalculatedDto> allSteps)
    {
        var required = allSteps.Where(s => s.IsRequired).OrderBy(s => s.Level).ToList();
        if (required.Any(s => s.Status == DecisionType.Rejected)) return ApprovalStatus.Rejected;
        if (required.Count > 0 && required.All(s => s.Status == DecisionType.Approved)) return ApprovalStatus.Approved;
        return required.Any(s => s.Status == DecisionType.Approved)
            ? ApprovalStatus.InProgress
            : ApprovalStatus.Pending;
    }

    public override async Task NotifyStepCompletedAsync(
        OTRequestSubject subject,
        ApprovalStepCalculatedDto completedStep,
        bool isFullyApproved,
        CancellationToken ct)
    {
        try
        {
            if (isFullyApproved)
            {
                await NotifyEmployeeAsync(subject, ApprovalStatus.Approved, ct);
                await NotifyEmployeeInAppAsync(subject, ApprovalStatus.Approved, ct);
                return;
            }

            if (completedStep.Status == DecisionType.Rejected)
            {
                await NotifyEmployeeAsync(subject, ApprovalStatus.Rejected, ct);
                await NotifyEmployeeInAppAsync(subject, ApprovalStatus.Rejected, ct);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OT-PROVIDER] NotifyStepCompleted lỗi: OTId={Id}", subject.RequestId);
        }
    }

    private async Task NotifyEmployeeAsync(OTRequestSubject subject, ApprovalStatus status, CancellationToken ct)
    {
        var empEmail = await GetEmployeeEmailAsync(subject.EmployeeCode, ct);
        if (string.IsNullOrWhiteSpace(empEmail)) return;

        var templateCode = status == ApprovalStatus.Approved ? "OT_APPROVED" : "OT_REJECTED";
        await _email.QueueEmail(empEmail, templateCode, new
        {
            subject.RequestId,
            subject.EmployeeCode,
            OTDate = subject.OTDate.ToString("dd/MM/yyyy"),
            subject.TotalOTHours,
            Status = status.ToDisplayName()
        }, ct);
    }

    public override async Task<PendingApprovalItemDto> ToPendingItemAsync(
        OTRequestSubject subject,
        List<ApprovalStepCalculatedDto> steps,
        bool canApprove,
        CancellationToken ct)
    {
        var deptName = string.IsNullOrEmpty(subject.DeptCode)
            ? null
            : await _uow.Repository<F03Department>().Query().AsNoTracking()
                .Where(d => d.DeptCode == subject.DeptCode)
                .Select(d => d.DeptName)
                .FirstOrDefaultAsync(ct);

        return new PendingApprovalItemDto
        {
            RequestId = subject.RequestId,
            Kind = subject.Module,
            EmployeeCode = subject.EmployeeCode,
            EmployeeName = subject.EmployeeName ?? string.Empty,
            DeptCode = subject.DeptCode ?? string.Empty,
            DeptName = deptName ?? string.Empty,
            FromDate = subject.OTDate,
            ToDate = subject.OTDate,
            TotalUnits = subject.TotalOTHours,
            ApprovalSteps = steps,
            CanApprove = canApprove
        };
    }

    private async Task<(string? EmployeeName, string? PositionCode)?> GetEmployeeInfoAsync(
        string employeeCode, CancellationToken ct)
    {
        var emp = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(e => e.EmployeeCode == employeeCode)
            .Select(e => new { e.EmployeeName, e.PositionCode })
            .FirstOrDefaultAsync(ct);

        return emp == null ? null : (emp.EmployeeName, emp.PositionCode);
    }

    private Task<string?> GetEmployeeEmailAsync(string employeeCode, CancellationToken ct)
        => _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(e => e.EmployeeCode == employeeCode)
            .Select(e => e.EmailAddress)
            .FirstOrDefaultAsync(ct);
}
