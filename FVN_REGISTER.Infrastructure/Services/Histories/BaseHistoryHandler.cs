using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities;
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

    public abstract string Kind { get; }

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

    protected abstract string[] ActiveStatuses { get; }
    protected abstract string CancelledStatus { get; }

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

        var pendingSteps = await Db.F03ApprovalSteps
            .Where(s => s.RequestId == id
                     && s.RequestType == Kind
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
        return await Db.F03ApprovalSteps
            .AsNoTracking()
            .Where(s => s.RequestType == Kind && s.RequestId == requestId)
            .OrderBy(s => s.Level)
            .Select(s => MapStep(s))
            .ToListAsync(ct);
    }

    protected async Task<Dictionary<int, List<ApprovalStepDto>>> GetApprovalStepsMapAsync(
        List<int> requestIds,
        CancellationToken ct)
    {
        var steps = await Db.F03ApprovalSteps
            .AsNoTracking()
            .Where(s => s.RequestType == Kind && requestIds.Contains(s.RequestId))
            .OrderBy(s => s.Level)
            .Select(s => new { s.RequestId, Step = MapStep(s) })
            .ToListAsync(ct);

        return steps
            .GroupBy(x => x.RequestId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Step).ToList());
    }

    private static ApprovalStepDto MapStep(F03ApprovalStep s) => new()
    {
        RequestType = s.RequestType,
        RequestId = s.RequestId,
        Level = s.Level,
        RoleName = s.RoleName,
        ApproverCode = s.ApproverCode,
        ApproverName = s.ApproverName,
        ApproverEmail = s.ApproverEmail,
        IsApproved = s.Approved,
        ApproveTime = s.ApprovedAt,
        Comment = s.Comment,
        IsRequired = s.Required,
        IsSkipped = !s.Required && s.Approved == null,
        IsOverriddenByAdmin = s.IsOverriddenByAdmin,
        OverriddenByName = s.OverriddenByName,
        OverriddenAt = s.OverriddenAt
    };

    protected static bool CanUserCancel(string ownerEmployeeCode, UserIdentityDto user)
        => ownerEmployeeCode == user.EmployeeCode
        || user.IsAdmin()
        || user.IsSuperAdmin();
}
