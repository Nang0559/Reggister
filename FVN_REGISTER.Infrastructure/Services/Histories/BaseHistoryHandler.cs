using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Histories;

public abstract class BaseHistoryHandler<TRequest> : IHistoryHandler
    where TRequest : class, IHistoryRequestEntity
{
    protected readonly FVNWEBAPPContext Db;

    protected BaseHistoryHandler(FVNWEBAPPContext db)
    {
        Db = db;
    }

    protected abstract RequestModule ModuleKind { get; }

    public string Kind => ModuleKind.ToCode();

    public abstract Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
        HistoryFilterDto filter,
        UserIdentityDto user,
        CancellationToken ct);

    public abstract Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
        int id,
        UserIdentityDto user,
        CancellationToken ct);

    public abstract Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
        int year,
        UserIdentityDto user,
        CancellationToken ct);

    protected virtual ApprovalStatus[] ActiveStatuses =>
    [
        ApprovalStatus.Draft,
        ApprovalStatus.Pending,
        ApprovalStatus.InProgress,
        ApprovalStatus.Escalated,
        ApprovalStatus.NeedsRevision
    ];

    protected virtual ApprovalStatus CancelledStatus => ApprovalStatus.Cancelled;

    public virtual async Task<ServiceResult> CancelAsync(
        int id,
        string reason,
        UserIdentityDto user,
        CancellationToken ct)
    {
        var request = await Db.Set<TRequest>()
            .FirstOrDefaultAsync(BuildIdPredicate(id), ct);

        if (request == null)
            return ServiceResult.Fail("Không tìm thấy đơn.");

        if (!CanUserCancel(request.EmployeeCode, user))
            return ServiceResult.Fail("Không có quyền hủy đơn này.");

        if (!ActiveStatuses.Contains(request.RequestStatus))
            return ServiceResult.Fail("Đơn không thể hủy ở trạng thái hiện tại.");

        request.IsActive = false;
        request.RequestStatus = CancelledStatus;
        request.ModifiedAt = DateTime.Now;
        request.ModifiedBy = user.UserId;

        var pendingSteps = await Db.ApprovalSteps
            .Where(s => s.RequestId == id
                     && s.RequestType == ModuleKind
                     && s.Approved == null)
            .ToListAsync(ct);

        foreach (var step in pendingSteps)
        {
            step.Required = false;
            step.Comment = $"Đơn đã bị hủy bởi {user.UserName}: {reason}";
        }

        await Db.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã hủy đơn thành công.");
    }

    protected abstract System.Linq.Expressions.Expression<Func<TRequest, bool>> BuildIdPredicate(int id);

    protected async Task<List<ApprovalStepDto>> GetApprovalStepsAsync(
        int requestId,
        CancellationToken ct)
    {
        return await Db.ApprovalSteps
            .AsNoTracking()
            .Where(s => s.RequestType == ModuleKind && s.RequestId == requestId)
            .OrderBy(s => s.Level)
            .Select(s => MapStep(s))
            .ToListAsync(ct);
    }

    protected async Task<Dictionary<int, List<ApprovalStepDto>>> GetApprovalStepsMapAsync(
        List<int> requestIds,
        CancellationToken ct)
    {
        var steps = await Db.ApprovalSteps
            .AsNoTracking()
            .Where(s => s.RequestType == ModuleKind && requestIds.Contains(s.RequestId))
            .OrderBy(s => s.Level)
            .Select(s => new { s.RequestId, Step = MapStep(s) })
            .ToListAsync(ct);

        return steps
            .GroupBy(x => x.RequestId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Step).ToList());
    }

    private static ApprovalStepDto MapStep(F03ApprovalStep s) => new()
    {
        Level = s.Level,
        RoleName = s.RoleName,
        ApproverCode = s.ApproverCode,
        ApproverName = s.ApproverName,
        ApproverEmail = s.ApproverEmail,
        IsApproved = s.Approved,
        ApproveTime = s.ApprovedAt,
        Comment = s.Comment,
        IsRequired = s.Required,
        IsOverriddenByAdmin = s.IsOverriddenByAdmin,
        OverriddenByName = s.OverriddenByName,
        OverriddenAt = s.OverriddenAt
    };

    protected static bool CanUserCancel(string ownerEmployeeCode, UserIdentityDto user)
        => ownerEmployeeCode == user.EmployeeCode || user.IsAdmin;
}
