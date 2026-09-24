using FVN_REGISTER.Application.Interfaces.Approvals;

using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Models.Dtos.Notifications;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace FVN_REGISTER.Infrastructure.Services.Approvals;
public sealed class EquipmentApprovalProvider : BaseApprovalProvider<EquipmentRequestSubject, EquipmentApprovalProvider>, IApprovalProvider<EquipmentRequestSubject>
{
    public override RequestModule RequestType => RequestModule.Equipment;
    private readonly IActionItemWriter _actionWriter;
    private readonly INotificationService _notifications;

    public EquipmentApprovalProvider(
        IUnitOfWork uow,
        IEmailService email,
        IApprovalNotificationService notification,
        IEmployeeUserResolver userResolver,
        IApprovalRouteService routeService,
        IApprovalSelectionService selectionService,
        IActionItemWriter actionWriter,
        INotificationService notifications,
        ILogger<EquipmentApprovalProvider> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(uow, email, notification, userResolver, routeService, selectionService, logger, options) { _actionWriter = actionWriter; _notifications = notifications; }
    public override async Task<EquipmentRequestSubject?> GetSubjectAsync(int requestId, CancellationToken ct)
    {
        var x = await _uow.Repository<F03EquipmentRequest>().Query().AsNoTracking().FirstOrDefaultAsync(r => r.Id == requestId && r.IsActive == true, ct); if (x == null) return null;
        var e = await _uow.Repository<F03Employee>().Query().AsNoTracking().Where(v => v.EmployeeCode == x.EmployeeCode).Select(v => new { v.EmployeeName, v.PositionCode }).FirstOrDefaultAsync(ct);
        return EquipmentRequestSubject.From(x, e?.EmployeeName, e?.PositionCode);
    }
    public override async Task<List<EquipmentRequestSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct)
    {
        var rows = await _uow.Repository<F03EquipmentRequest>().Query().AsNoTracking().Where(x => requestIds.Contains(x.Id) && x.IsActive == true).ToListAsync(ct);
        var codes = rows.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking().Where(x => codes.Contains(x.EmployeeCode)).Select(x => new { x.EmployeeCode, x.EmployeeName, x.PositionCode }).ToDictionaryAsync(x => x.EmployeeCode, x => x, ct);
        return rows.Select(x => { employees.TryGetValue(x.EmployeeCode, out var e); return EquipmentRequestSubject.From(x, e?.EmployeeName, e?.PositionCode); }).ToList();
    }
    public override async Task ApplyOverallStatusAsync(int requestId, IReadOnlyList<ApprovalStepDto> allSteps, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03EquipmentRequest>().Query().FirstOrDefaultAsync(x => x.Id == requestId, ct); if (entity == null) return;
        var required = allSteps.Where(x => x.IsRequired).ToList();
        entity.RequestStatus = required.Any(x => x.Decision == DecisionType.Rejected) ? ApprovalStatus.Rejected : required.Count > 0 && required.All(x => x.Decision == DecisionType.Approved) ? ApprovalStatus.Approved : required.Any(x => x.Decision == DecisionType.Approved) ? ApprovalStatus.InProgress : ApprovalStatus.Pending;
        if (entity.RequestStatus == ApprovalStatus.Approved)
        {
            if (entity.RequestKind == EquipmentRequestKind.Registration && entity.AssetId == null)
            {
                var asset = new F03EquipmentAsset { EquipmentCode = $"EQP-{Guid.NewGuid():N}"[..30], EquipmentName = entity.EquipmentName, Specification = entity.Specification, SerialNumber = entity.SerialNumber, AssetCode = entity.AssetCode, PurchasePrice = entity.PurchasePrice, PurchaseDate = entity.PurchaseDate!.Value, ExpectedDepreciationDate = entity.ExpectedDepreciationDate!.Value, DeptCode = entity.DeptCode!, Location = entity.Location, QrToken = entity.QrToken, IsQrActive = true, Note = entity.Note, ResponsibleEmployeeCode = entity.ResponsibleEmployeeCode ?? entity.EmployeeCode, OperatingResponsibleEmployeeCode = entity.ResponsibleEmployeeCode, OperatingResponsibleDeptCode = entity.ResponsibleEmployeeCode == null ? entity.DeptCode : null, OperatingResponsibleAssignedAt = entity.ResponsibleEmployeeCode == null ? null : DateTime.UtcNow, ResponsibleApproverEmployeeCode = entity.SelectedApproverCode, ResponsibleAssignedAt = DateTime.UtcNow, CreatedBy = entity.OperatorUserId };
                await _uow.Repository<F03EquipmentAsset>().AddAsync(asset, ct); await _uow.SaveChangesAsync(ct); entity.AssetId = asset.Id;
            }
            else if (entity.RequestKind == EquipmentRequestKind.Repair && entity.AssetId.HasValue)
            {
                await CreateRepairExecutionTasksAsync(entity, ct);
            }
        }
        await _uow.SaveChangesAsync(ct);
    }

    private async Task CreateRepairExecutionTasksAsync(F03EquipmentRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.RepairFeedback) || request.RepairCompletedAt.HasValue)
            return;

        var requester = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(e => e.EmployeeCode == request.EmployeeCode)
            .Select(e => new { e.Id, e.EmployeeName })
            .FirstOrDefaultAsync(ct);
        if (requester == null) return;

        var employees = new List<(int Id, string Code, string Name, string Email, int? UserId)>();
        if (!string.IsNullOrWhiteSpace(request.RepairAssigneeEmployeeCode))
        {
            var row = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                .Where(e => e.EmployeeCode == request.RepairAssigneeEmployeeCode && e.IsActive == true && e.EndWorkingDate == null)
                .Select(e => new { e.Id, e.EmployeeCode, e.EmployeeName, e.EmailAddress })
                .FirstOrDefaultAsync(ct);
            if (row != null)
            {
                var uid = await _uow.Repository<F03User>().Query().AsNoTracking()
                    .Where(u => u.EmployeeCode == row.EmployeeCode && u.LockoutEndDate == null)
                    .Select(u => (int?)u.Id).FirstOrDefaultAsync(ct);
                employees.Add((row.Id,row.EmployeeCode,row.EmployeeName,row.EmailAddress,uid));
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.RepairResponsibleDeptCode))
        {
            var rows = await _uow.Repository<F03Employee>().Query().AsNoTracking()
                .Where(e => e.DeptCode == request.RepairResponsibleDeptCode && e.IsActive == true && e.EndWorkingDate == null)
                .Select(e => new { e.Id, e.EmployeeCode, e.EmployeeName, e.EmailAddress })
                .ToListAsync(ct);
            var codes = rows.Select(x => x.EmployeeCode).ToList();
            var users = await _uow.Repository<F03User>().Query().AsNoTracking()
                .Where(u => codes.Contains(u.EmployeeCode) && u.LockoutEndDate == null)
                .Select(u => new { u.EmployeeCode, u.Id }).ToListAsync(ct);
            foreach (var row in rows)
                employees.Add((row.Id,row.EmployeeCode,row.EmployeeName,row.EmailAddress,users.FirstOrDefault(u => u.EmployeeCode == row.EmployeeCode)?.Id));
        }

        if (employees.Count == 0) return;

        var payload = System.Text.Json.JsonSerializer.Serialize(new { RequestId = request.Id, AssetId = request.AssetId, AssignmentType = request.RepairAssigneeEmployeeCode != null ? "Employee" : "Department" });
        foreach (var employee in employees)
        {
            await _actionWriter.EnsureOpenAsync(new FVN_REGISTER.Application.Models.Actions.ActionItemDraft(
                "EQUIPMENT", request.Id.ToString(), requester.Id, employee.Id, employee.UserId,
                request.RepairDate.HasValue ? DateOnly.FromDateTime(request.RepairDate.Value) : DateOnly.FromDateTime(DateTime.Today),
                "REPAIR_EXECUTION", $"Sửa chữa thiết bị {request.EquipmentName}",
                request.RepairContent, 1, 100, null, $"/equipment/repair/{request.Id}", request.AssetCode, payload,
                "EQUIPMENT_REPAIR", request.Id.ToString(), request.OperatorUserId), ct);

            if (employee.UserId.HasValue)
                await _notifications.CreateAsync(new CreateNotificationDto
                {
                    UserId = employee.UserId.Value,
                    EmployeeCode = employee.Code,
                    Title = $"Nhiệm vụ sửa chữa: {request.EquipmentName}",
                    Body = $"Yêu cầu #{request.Id}: {request.RepairContent}",
                    ActionUrl = $"/equipment/repair/{request.Id}",
                    Module = RequestModule.Equipment,
                    RelatedRequestId = request.Id,
                    Action = NotificationAction.Pending,
                    IsHighPriority = true,
                    NotificationType = "EQUIPMENT_REPAIR_EXECUTION"
                }, ct);

            if (!string.IsNullOrWhiteSpace(employee.Email))
                await _email.QueueEmail(employee.Email, "EQUIPMENT_REPAIR_ASSIGNED", new
                {
                    EmployeeName = employee.Name, request.Id, request.EquipmentName,
                    request.RepairContent, request.AssetCode,
                    RepairResponsibleDeptCode = request.RepairResponsibleDeptCode
                }, ct);
        }
    }
    public override async Task NotifyStepCompletedAsync(EquipmentRequestSubject subject, ApprovalStepDto completedStep, bool isFullyApproved, CancellationToken ct)
    {
        if (!isFullyApproved && completedStep.Decision != DecisionType.Rejected) return;
        var email = await _uow.Repository<F03Employee>().Query().AsNoTracking().Where(x => x.EmployeeCode == subject.EmployeeCode).Select(x => x.EmailAddress).FirstOrDefaultAsync(ct);
        if (!string.IsNullOrWhiteSpace(email)) await _email.QueueEmail(email, isFullyApproved ? "EQUIPMENT_APPROVED" : "EQUIPMENT_REJECTED", new { subject.RequestId, subject.RequestKind, subject.EquipmentName, subject.AssetCode, subject.RepairDate, Status = isFullyApproved ? "Approved" : "Rejected" }, ct);
        await NotifyEmployeeInAppAsync(subject, isFullyApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected, ct);
    }
    public override async Task<PendingApprovalItemDto> ToPendingItemAsync(EquipmentRequestSubject subject, List<ApprovalStepDto> steps, bool canApprove, CancellationToken ct)
    {
        var deptName = await _uow.Repository<F03Department>().Query().AsNoTracking().Where(x => x.DeptCode == subject.DeptCode).Select(x => x.DeptName).FirstOrDefaultAsync(ct);
        return new PendingApprovalItemDto { RequestId = subject.RequestId, Kind = subject.Module, EmployeeCode = subject.EmployeeCode, EmployeeName = subject.EmployeeName ?? string.Empty, DeptCode = subject.DeptCode ?? string.Empty, DeptName = deptName ?? string.Empty, FromDate = subject.RepairDate ?? DateTime.Now, ToDate = subject.RepairDate ?? DateTime.Now, TotalUnits = 1, ApprovalSteps = steps, CanApprove = canApprove };
    }
}
