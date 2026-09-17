using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public sealed class EquipmentApprovalProvider
    : BaseApprovalProvider<EquipmentRequestSubject, EquipmentApprovalProvider>, IApprovalProvider<EquipmentRequestSubject>
{
    public override RequestModule RequestType => RequestModule.Equipment;

    protected override IReadOnlyList<(int Level, string LevelName, string RoleName)> LevelDefs { get; } = new[]
    {
        (1, "Level 1", "EquipmentApprover"),
        (2, "Level 2", "EquipmentManager"),
        (3, "Level 3", "GM")
    };

    public EquipmentApprovalProvider(IUnitOfWork uow, IEmailService email,
        IApprovalNotificationService notification, IEmployeeUserResolver userResolver,
        ILogger<EquipmentApprovalProvider> logger, IOptionsMonitor<AuthDebugOptions> options)
        : base(uow, email, notification, userResolver, logger, options) { }

    protected override bool? ResolveRequired(int level, ApprovalBuildContext ctx) => true;

    public override async Task<List<ApprovalStepSnapshotDto>> BuildHierarchyAsync(ApprovalBuildContext ctx, CancellationToken ct)
    {
        var result = new List<ApprovalStepSnapshotDto>();
        foreach (var def in LevelDefs)
        {
            F03Approver? approver;
            if (def.Level == 1 && !string.IsNullOrWhiteSpace(ctx.SelectedApproverCode))
            {
                approver = await _uow.Repository<F03Approver>().Query().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.RequestType == RequestType && x.IsActive == true
                        && x.ApproverCode == ctx.SelectedApproverCode
                        && (x.ApproveForDeptCode == ctx.DeptCode || x.ApproveForDeptCode == Core.Constants.ApproveForDept.All), ct);
            }
            else
            {
                approver = await GetApproverForLevelAsync(def.Level, ctx.DeptCode, ct);
            }

            if (approver == null) continue;
            result.Add(new ApprovalStepSnapshotDto(def.Level, def.LevelName, def.RoleName,
                approver.ApproverCode, approver.ApproverName, approver.ApproverEmail, true));
        }
        return result;
    }

    public override async Task<EquipmentRequestSubject?> GetSubjectAsync(int requestId, CancellationToken ct)
    {
        var x = await _uow.Repository<F03EquipmentRequest>().Query().AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requestId && r.IsActive, ct);
        if (x == null) return null;
        var e = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(v => v.EmployeeCode == x.EmployeeCode)
            .Select(v => new { v.EmployeeName, v.PositionCode }).FirstOrDefaultAsync(ct);
        return EquipmentRequestSubject.From(x, e?.EmployeeName, e?.PositionCode);
    }

    public override async Task<List<EquipmentRequestSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct)
    {
        var rows = await _uow.Repository<F03EquipmentRequest>().Query().AsNoTracking()
            .Where(x => requestIds.Contains(x.Id) && x.IsActive).ToListAsync(ct);
        var codes = rows.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => codes.Contains(x.EmployeeCode))
            .Select(x => new { x.EmployeeCode, x.EmployeeName, x.PositionCode })
            .ToDictionaryAsync(x => x.EmployeeCode, x => x, ct);
        return rows.Select(x => { employees.TryGetValue(x.EmployeeCode, out var e); return EquipmentRequestSubject.From(x, e?.EmployeeName, e?.PositionCode); }).ToList();
    }

    public override async Task ApplyOverallStatusAsync(int requestId, IReadOnlyList<ApprovalStepCalculatedDto> allSteps, CancellationToken ct)
    {
        var entity = await _uow.Repository<F03EquipmentRequest>().Query().FirstOrDefaultAsync(x => x.Id == requestId, ct);
        if (entity == null) return;
        var required = allSteps.Where(x => x.IsRequired).ToList();
        entity.RequestStatus = required.Any(x => x.Status == DecisionType.Rejected)
            ? ApprovalStatus.Rejected
            : required.Count > 0 && required.All(x => x.Status == DecisionType.Approved)
                ? ApprovalStatus.Approved
                : required.Any(x => x.Status == DecisionType.Approved) ? ApprovalStatus.InProgress : ApprovalStatus.Pending;

        if (entity.RequestStatus == ApprovalStatus.Approved)
        {
            if (entity.RequestKind == EquipmentRequestKind.Registration)
            {
                var asset = new F03EquipmentAsset
                {
                    EquipmentCode = $"EQP-{Guid.NewGuid():N}"[..30],
                    EquipmentName = entity.EquipmentName,
                    Specification = entity.Specification,
                    SerialNumber = entity.SerialNumber,
                    AssetCode = entity.AssetCode,
                    PurchasePrice = entity.PurchasePrice,
                    PurchaseDate = entity.PurchaseDate!.Value,
                    ExpectedDepreciationDate = entity.ExpectedDepreciationDate!.Value,
                    DeptCode = entity.DeptCode!,
                    Location = entity.Location,
                    QrToken = entity.QrToken,
                    IsQrActive = true,
                    IsActive = true,
                    Note = entity.Note,
                    CreatedBy = entity.OperatorUserId
                };
                await _uow.Repository<F03EquipmentAsset>().AddAsync(asset, ct);
                await _uow.SaveChangesAsync(ct);
                entity.AssetId = asset.Id;
            }
            else if (entity.AssetId.HasValue && entity.RepairDate.HasValue)
            {
                var history = new F03EquipmentRepairHistory
                {
                    AssetId = entity.AssetId.Value,
                    RequestId = entity.Id,
                    RepairDate = entity.RepairDate.Value,
                    OperatorUserId = entity.OperatorUserId,
                    RepairCost = entity.RepairCost,
                    RepairContent = entity.RepairContent ?? string.Empty,
                    RepairVendor = entity.RepairVendor,
                    RepairResult = entity.RepairResult,
                    Note = entity.Note,
                    IsApproved = true,
                    CreatedBy = entity.OperatorUserId
                };
                await _uow.Repository<F03EquipmentRepairHistory>().AddAsync(history, ct);
            }
        }
        await _uow.SaveChangesAsync(ct);
    }

    public override async Task NotifyStepCompletedAsync(EquipmentRequestSubject subject, ApprovalStepCalculatedDto completedStep, bool isFullyApproved, CancellationToken ct)
    {
        if (!isFullyApproved && completedStep.Status != DecisionType.Rejected) return;
        var email = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => x.EmployeeCode == subject.EmployeeCode).Select(x => x.EmailAddress).FirstOrDefaultAsync(ct);
        if (!string.IsNullOrWhiteSpace(email))
            await _email.QueueEmail(email, isFullyApproved ? "EQUIPMENT_APPROVED" : "EQUIPMENT_REJECTED",
                new { subject.RequestId, subject.RequestKind, subject.EquipmentName, subject.AssetCode, subject.RepairDate, Status = isFullyApproved ? "Approved" : "Rejected" }, ct);
        await NotifyEmployeeInAppAsync(subject, isFullyApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected, ct);
    }

    public override async Task<PendingApprovalItemDto> ToPendingItemAsync(EquipmentRequestSubject subject, List<ApprovalStepCalculatedDto> steps, bool canApprove, CancellationToken ct)
    {
        var deptName = await _uow.Repository<F03Department>().Query().AsNoTracking()
            .Where(x => x.DeptCode == subject.DeptCode).Select(x => x.DeptName).FirstOrDefaultAsync(ct);
        return new PendingApprovalItemDto
        {
            RequestId = subject.RequestId, Kind = subject.Module, EmployeeCode = subject.EmployeeCode,
            EmployeeName = subject.EmployeeName ?? string.Empty, DeptCode = subject.DeptCode ?? string.Empty,
            DeptName = deptName ?? string.Empty, FromDate = subject.RepairDate ?? DateTime.Now,
            ToDate = subject.RepairDate ?? DateTime.Now, TotalUnits = 1,
            ApprovalSteps = steps, CanApprove = canApprove
        };
    }
}
