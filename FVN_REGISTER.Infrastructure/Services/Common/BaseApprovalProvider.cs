using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Common;

public abstract class BaseApprovalProvider<TSubject, TProvider> : BaseService<TProvider>
    where TSubject : IApprovalSubject where TProvider : class
{
    protected readonly IUnitOfWork _uow;
    protected readonly IEmailService _email;
    protected readonly IApprovalNotificationService _notification;
    protected readonly IEmployeeUserResolver _userResolver;
    private readonly IApprovalRouteService _routeService;
    private readonly IApprovalSelectionService _selectionService;

    protected BaseApprovalProvider(
        IUnitOfWork uow,
        IEmailService email,
        IApprovalNotificationService notification,
        IEmployeeUserResolver userResolver,
        IApprovalRouteService routeService,
        IApprovalSelectionService selectionService,
        ILogger<TProvider> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _uow = uow;
        _email = email;
        _notification = notification;
        _userResolver = userResolver;
        _routeService = routeService;
        _selectionService = selectionService;
    }

    public abstract RequestModule RequestType { get; }

    /// <summary>
    /// Builds the approval hierarchy for a request.
    /// Strict mode is used by command/submit flows; non-strict mode is used by
    /// read-only screens such as calendars so missing approval configuration
    /// does not turn an otherwise valid query into HTTP 500.
    /// </summary>
    // Backward-compatible interface implementation.
    // Existing IApprovalProvider<T> consumers keep the original two-argument contract;
    // command flows therefore remain strict by default.
    public virtual Task<List<ApprovalStepSnapshotDto>> BuildHierarchyAsync(
        ApprovalBuildContext ctx,
        CancellationToken ct)
        => BuildHierarchyAsync(ctx, ct, strict: true);

    public virtual async Task<List<ApprovalStepSnapshotDto>> BuildHierarchyAsync(
        ApprovalBuildContext ctx,
        CancellationToken ct,
        bool strict = true)
    {
        var routeResult = await _routeService.GetPreviewAsync(
            RequestType,
            ctx.EmployeeCode,
            ctx.DeptCode,
            ctx.PositionCode,
            ct);

        if (!routeResult.Success || routeResult.Data == null)
        {
            if (!strict)
            {
                Logger.LogWarning(
                    "Approval route unavailable for {RequestType}, EmployeeCode={EmployeeCode}, DeptCode={DeptCode}, PositionCode={PositionCode}: {Message}",
                    RequestType,
                    ctx.EmployeeCode,
                    ctx.DeptCode,
                    ctx.PositionCode,
                    routeResult.Message);
                return new List<ApprovalStepSnapshotDto>();
            }

            throw new InvalidOperationException(
                routeResult.Message ??
                $"Không xác định được luồng phê duyệt cho {RequestType}.");
        }

        var route = routeResult.Data;

        var selections = await _selectionService.GetAsync(
            RequestType,
            ctx.RequestId,
            ct);

        var selectedByLevel = selections
            .GroupBy(x => x.Level)
            .ToDictionary(
                x => x.Key,
                x => x.Last().ApproverCode);

        var result = new List<ApprovalStepSnapshotDto>();

        foreach (var level in route.Levels
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Level))
        {
            if (!level.Required)
                continue;

            if (level.Candidates.Count == 0)
            {
                if (!strict)
                {
                    Logger.LogWarning(
                        "No approval candidates for {RequestType}, Level={Level}, LevelName={LevelName}, EmployeeCode={EmployeeCode}, DeptCode={DeptCode}, PositionCode={PositionCode}.",
                        RequestType,
                        level.Level,
                        level.LevelName,
                        ctx.EmployeeCode,
                        ctx.DeptCode,
                        ctx.PositionCode);
                    continue;
                }

                throw new InvalidOperationException(
                    $"Không tìm thấy người phê duyệt cho " +
                    $"{RequestType}, {level.LevelName} (Level {level.Level}).");
            }

            if (!selectedByLevel.TryGetValue(
                    level.Level,
                    out var selectedCode) ||
                string.IsNullOrWhiteSpace(selectedCode))
            {
                if (!strict)
                {
                    Logger.LogDebug(
                        "No selected approver for {RequestType}, Level={Level}, RequestId={RequestId}; skipping in non-strict mode.",
                        RequestType,
                        level.Level,
                        ctx.RequestId);
                    continue;
                }

                throw new InvalidOperationException(
                    $"Chưa chọn người phê duyệt cho " +
                    $"{RequestType}, {level.LevelName} (Level {level.Level}).");
            }

            var selected = level.Candidates.FirstOrDefault(
                x => string.Equals(
                    x.ApproverCode,
                    selectedCode,
                    StringComparison.OrdinalIgnoreCase));

            if (selected == null)
            {
                if (!strict)
                {
                    Logger.LogWarning(
                        "Selected approver {ApproverCode} is not valid for {RequestType}, Level={Level}, RequestId={RequestId}; skipping in non-strict mode.",
                        selectedCode,
                        RequestType,
                        level.Level,
                        ctx.RequestId);
                    continue;
                }

                throw new InvalidOperationException(
                    $"Người phê duyệt [{selectedCode}] " +
                    $"không thuộc danh sách hợp lệ của " +
                    $"{RequestType}, Level {level.Level}.");
            }

            result.Add(new ApprovalStepSnapshotDto(
                level.Level,
                level.LevelName,
                level.RoleName,
                selected.ApproverCode,
                selected.ApproverName,
                selected.ApproverEmail,
                true));
        }

        if (result.Count == 0)
        {
            if (!strict)
            {
                Logger.LogWarning(
                    "Approval hierarchy is empty for {RequestType}, EmployeeCode={EmployeeCode}, DeptCode={DeptCode}, PositionCode={PositionCode}.",
                    RequestType,
                    ctx.EmployeeCode,
                    ctx.DeptCode,
                    ctx.PositionCode);
                return result;
            }

            throw new InvalidOperationException(
                $"Không xác định được cấp phê duyệt cho " +
                $"{RequestType} / chức vụ {ctx.PositionCode}.");
        }

        return result;
    }

    public virtual async Task<ApprovalSnapshotDto> BuildSnapshotAsync(
        TSubject subject,
        ApprovalBuildContext ctx,
        CancellationToken ct)
        => new(subject.RequestId, RequestType, DateTime.Now, await BuildHierarchyAsync(ctx, ct));

    protected async Task NotifyEmployeeInAppAsync(
        TSubject subject,
        ApprovalStatus status,
        CancellationToken ct)
    {
        var creatorUserId = await _userResolver.ResolveUserIdAsync(subject.EmployeeCode, ct);
        if (creatorUserId is null or <= 0) return;

        await _notification.NotifyCreatorInAppAsync(
            creatorUserId.Value,
            subject.EmployeeCode,
            status.ToString(),
            subject.RequestId,
            RequestType,
            ct);
    }

    public abstract Task<TSubject?> GetSubjectAsync(int requestId, CancellationToken ct);
    public abstract Task<List<TSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct);
    public abstract Task ApplyOverallStatusAsync(int requestId, IReadOnlyList<ApprovalStepDto> allSteps, CancellationToken ct);
    public abstract Task NotifyStepCompletedAsync(TSubject subject, ApprovalStepDto completedStep, bool isFullyApproved, CancellationToken ct);
    public abstract Task<PendingApprovalItemDto> ToPendingItemAsync(TSubject subject, List<ApprovalStepDto> steps, bool canApprove, CancellationToken ct);
}
