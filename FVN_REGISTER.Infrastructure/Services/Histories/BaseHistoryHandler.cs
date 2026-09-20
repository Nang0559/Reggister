using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
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

    public RequestModule Kind => ModuleKind;   // ✅ khớp interface, không cần ToCode() nữa

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

        // Cancellation is represented by the request status itself. The legacy
        // F03ApprovalSteps table is no longer part of the approval pipeline and
        // must not be mutated here. Snapshot steps are immutable; ApprovalEngine
        // reads the request subject/status and therefore cancelled requests are
        // excluded from the pending inbox.
        await Db.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã hủy đơn thành công.");
    }

    protected abstract System.Linq.Expressions.Expression<Func<TRequest, bool>> BuildIdPredicate(int id);

    protected async Task<List<ApprovalStepDto>> GetApprovalStepsAsync(
        int requestId,
        CancellationToken ct)
    {
        var snapshot = await Db.ApprovalSnapshots
            .AsNoTracking()
            .Include(s => s.Steps)
            .FirstOrDefaultAsync(
                s => s.RequestType == ModuleKind && s.RequestId == requestId, ct);

        if (snapshot == null)
            return new();

        var histories = await Db.ApprovalHistories
            .AsNoTracking()
            .Where(h => h.RequestType == ModuleKind && h.RequestId == requestId)
            .ToListAsync(ct);

        return ApprovalStepMapper.MapToList(snapshot.Steps, histories);
    }

    protected async Task<Dictionary<int, List<ApprovalStepDto>>> GetApprovalStepsMapAsync(
        List<int> requestIds,
        CancellationToken ct)
    {
        if (requestIds.Count == 0)
            return new();

        var snapshots = await Db.ApprovalSnapshots
            .AsNoTracking()
            .Include(s => s.Steps)
            .Where(s => s.RequestType == ModuleKind && requestIds.Contains(s.RequestId))
            .ToListAsync(ct);

        var histories = await Db.ApprovalHistories
            .AsNoTracking()
            .Where(h => h.RequestType == ModuleKind && requestIds.Contains(h.RequestId))
            .ToListAsync(ct);

        var historyByRequest = histories
            .GroupBy(h => h.RequestId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return snapshots.ToDictionary(
            s => s.RequestId,
            s => ApprovalStepMapper.MapToList(
                s.Steps,
                historyByRequest.GetValueOrDefault(s.RequestId, new())));
    }

    protected static bool CanUserCancel(string ownerEmployeeCode, UserIdentityDto user)
        => ownerEmployeeCode == user.EmployeeCode || user.IsAdmin;
}
