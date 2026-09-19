
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;

using FVN_REGISTER.Contract.Dtos.Approvals;

using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public sealed class TripApprovalProvider
    : BaseApprovalProvider<TripRequestSubject, TripApprovalProvider>, IApprovalProvider<TripRequestSubject>
{
    public override RequestModule RequestType => RequestModule.Trip;

    public TripApprovalProvider(
        IUnitOfWork uow,
        IEmailService email,
        IApprovalNotificationService notification,
        IEmployeeUserResolver userResolver,
        IApprovalRouteService routeService,
        IApprovalSelectionService selectionService,
        ILogger<TripApprovalProvider> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(uow, email, notification, userResolver, routeService, selectionService, logger, options) { }

    public override async Task<TripRequestSubject?> GetSubjectAsync(int requestId, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03TripRequest>().Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);
        if (entity == null) return null;

        var info = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(e => e.EmployeeCode == entity.EmployeeCode)
            .Select(e => new { e.EmployeeName, e.DeptCode, e.PositionCode })
            .FirstOrDefaultAsync(ct);

        return TripRequestSubject.From(entity, info?.EmployeeName, info?.PositionCode);
    }

    public override async Task<List<TripRequestSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct)
    {
        var entities = await _uow.Repository<F03TripRequest>().Query().AsNoTracking()
            .Where(x => requestIds.Contains(x.Id) && x.IsActive == true).ToListAsync(ct);
        var codes = entities.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => codes.Contains(x.EmployeeCode))
            .Select(x => new { x.EmployeeCode, x.EmployeeName, x.PositionCode })
            .ToDictionaryAsync(x => x.EmployeeCode, x => x, ct);

        return entities.Select(x =>
        {
            employees.TryGetValue(x.EmployeeCode, out var e);
            return TripRequestSubject.From(x, e?.EmployeeName, e?.PositionCode);
        }).ToList();
    }

    public override async Task ApplyOverallStatusAsync(
        int requestId, IReadOnlyList<ApprovalStepDto> allSteps, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03TripRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == requestId, ct);
        if (entity == null) return;

        var required = allSteps.Where(x => x.IsRequired).ToList();
        entity.RequestStatus = required.Any(x => x.Decision == DecisionType.Rejected)
            ? ApprovalStatus.Rejected
            : required.Count > 0 && required.All(x => x.Decision == DecisionType.Approved)
                ? ApprovalStatus.Approved
                : required.Any(x => x.Decision == DecisionType.Approved)
                    ? ApprovalStatus.InProgress
                    : ApprovalStatus.Pending;

        await _uow.SaveChangesAsync(ct);
    }

    public override async Task NotifyStepCompletedAsync(
        TripRequestSubject subject,
        ApprovalStepDto completedStep,
        bool isFullyApproved,
        CancellationToken ct)
    {
        var status = isFullyApproved
            ? ApprovalStatus.Approved
            : completedStep.Decision == DecisionType.Rejected
                ? ApprovalStatus.Rejected
                : (ApprovalStatus?)null;

        if (status == null) return;

        var email = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => x.EmployeeCode == subject.EmployeeCode)
            .Select(x => x.EmailAddress)
            .FirstOrDefaultAsync(ct);

        if (!string.IsNullOrWhiteSpace(email))
        {
            await _email.QueueEmail(email,
                status == ApprovalStatus.Approved ? "TRIP_APPROVED" : "TRIP_REJECTED",
                new
                {
                    subject.RequestId,
                    subject.EmployeeCode,
                    subject.TripCode,
                    subject.StartDate,
                    subject.EndDate,
                    subject.Destination,
                    Status = status.Value.ToDisplayName()
                }, ct);
        }

        await NotifyEmployeeInAppAsync(subject, status.Value, ct);
    }

    public override async Task<PendingApprovalItemDto> ToPendingItemAsync(
        TripRequestSubject subject,
        List<ApprovalStepDto> steps,
        bool canApprove,
        CancellationToken ct)
    {
        var deptName = string.IsNullOrWhiteSpace(subject.DeptCode)
            ? null
            : await _uow.Repository<F03Department>().Query().AsNoTracking()
                .Where(x => x.DeptCode == subject.DeptCode)
                .Select(x => x.DeptName)
                .FirstOrDefaultAsync(ct);

        return new PendingApprovalItemDto
        {
            RequestId = subject.RequestId,
            Kind = subject.Module,
            EmployeeCode = subject.EmployeeCode,
            EmployeeName = subject.EmployeeName ?? string.Empty,
            DeptCode = subject.DeptCode ?? string.Empty,
            DeptName = deptName ?? string.Empty,
            FromDate = subject.StartDate,
            ToDate = subject.EndDate,
            TotalUnits = (decimal)(subject.EndDate.Date - subject.StartDate.Date).TotalDays + 1,
            ApprovalSteps = steps,
            CanApprove = canApprove
        };
    }
}
