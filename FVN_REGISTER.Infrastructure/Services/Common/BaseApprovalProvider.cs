using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Utils;
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
    /// Legacy hierarchy API. Command flows should use the ServiceResult overload
    /// so business/configuration errors are returned to the caller instead of thrown.
    /// </summary>
    public virtual async Task<List<ApprovalStepSnapshotDto>> BuildHierarchyAsync(
        ApprovalBuildContext ctx,
        CancellationToken ct)
    {
        var result = await BuildHierarchyResultAsync(ctx, ct, strict: true);
        return result.Success && result.Data != null
            ? result.Data
            : new List<ApprovalStepSnapshotDto>();
    }

    public virtual async Task<ServiceResult<List<ApprovalStepSnapshotDto>>> BuildHierarchyResultAsync(
        ApprovalBuildContext ctx,
        CancellationToken ct,
        bool strict = true)
    {
        var routeResult = await _routeService.GetPreviewAsync(
            RequestType, ctx.EmployeeCode, ct);

        if (!routeResult.Success || routeResult.Data == null)
        {
            if (!strict)
            {
                Logger.LogWarning(
                    "Approval route unavailable for {RequestType}, EmployeeCode={EmployeeCode}, DeptCode={DeptCode}, PositionCode={PositionCode}: {Message}",
                    RequestType, ctx.EmployeeCode, ctx.DeptCode, ctx.PositionCode, routeResult.Message);

                return ServiceResult<List<ApprovalStepSnapshotDto>>.Ok(
                    new List<ApprovalStepSnapshotDto>(), routeResult.Message);
            }

            return ServiceResult<List<ApprovalStepSnapshotDto>>.Fail(
                routeResult.Message ??
                $"Không xác định được luồng phê duyệt cho {RequestType}.");
        }

        var route = routeResult.Data;

        var selections = await _selectionService.GetAsync(
            RequestType, ctx.RequestId, ct);

        var selectedByLevel = selections
            .GroupBy(x => x.Level)
            .ToDictionary(x => x.Key, x => x.Last().ApproverCode);

        var result = new List<ApprovalStepSnapshotDto>();

        foreach (var level in route.Levels
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Level))
        {
            if (!level.Required)
                continue;

            if (level.Candidates.Count == 0)
            {
                var message =
                    $"Không tìm thấy người phê duyệt cho {RequestType}, " +
                    $"{level.LevelName} (Level {level.Level}).";

                if (!strict)
                {
                    Logger.LogWarning(
                        "{Message} EmployeeCode={EmployeeCode}, DeptCode={DeptCode}, PositionCode={PositionCode}",
                        message, ctx.EmployeeCode, ctx.DeptCode, ctx.PositionCode);
                    continue;
                }

                return ServiceResult<List<ApprovalStepSnapshotDto>>.Fail(message);
            }

            if (!selectedByLevel.TryGetValue(level.Level, out var selectedCode) ||
                string.IsNullOrWhiteSpace(selectedCode))
            {
                var message =
                    $"Chưa chọn người phê duyệt cho {RequestType}, " +
                    $"{level.LevelName} (Level {level.Level}).";

                if (!strict)
                {
                    Logger.LogDebug(
                        "{Message} RequestId={RequestId}", message, ctx.RequestId);
                    continue;
                }

                return ServiceResult<List<ApprovalStepSnapshotDto>>.Fail(message);
            }

            var selected = level.Candidates.FirstOrDefault(
                x => string.Equals(
                    x.ApproverCode, selectedCode, StringComparison.OrdinalIgnoreCase));

            if (selected == null)
            {
                var message =
                    $"Người phê duyệt [{selectedCode}] không thuộc danh sách hợp lệ " +
                    $"của {RequestType}, Level {level.Level}.";

                if (!strict)
                {
                    Logger.LogWarning(
                        "{Message} RequestId={RequestId}", message, ctx.RequestId);
                    continue;
                }

                return ServiceResult<List<ApprovalStepSnapshotDto>>.Fail(message);
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
            var message =
                $"Không xác định được cấp phê duyệt cho " +
                $"{RequestType} / chức vụ {ctx.PositionCode}.";

            if (!strict)
            {
                Logger.LogWarning("{Message}", message);
                return ServiceResult<List<ApprovalStepSnapshotDto>>.Ok(result, message);
            }

            return ServiceResult<List<ApprovalStepSnapshotDto>>.Fail(message);
        }

        return ServiceResult<List<ApprovalStepSnapshotDto>>.Ok(result);
    }

    public virtual async Task<ApprovalSnapshotDto> BuildSnapshotAsync(
        TSubject subject,
        ApprovalBuildContext ctx,
        CancellationToken ct)
    {
        var result = await BuildSnapshotResultAsync(subject, ctx, ct);
        return result.Data ?? new ApprovalSnapshotDto(
            subject.RequestId,
            RequestType,
            DateTime.Now,
            new List<ApprovalStepSnapshotDto>());
    }

    public virtual async Task<ServiceResult<ApprovalSnapshotDto>> BuildSnapshotResultAsync(
        TSubject subject,
        ApprovalBuildContext ctx,
        CancellationToken ct)
    {
        var hierarchy = await BuildHierarchyResultAsync(ctx, ct, strict: true);
        if (!hierarchy.Success || hierarchy.Data == null)
            return ServiceResult<ApprovalSnapshotDto>.Fail(
                hierarchy.Message ?? $"Không thể khởi tạo luồng duyệt cho {RequestType}.");

        return ServiceResult<ApprovalSnapshotDto>.Ok(
            new ApprovalSnapshotDto(
                subject.RequestId,
                RequestType,
                DateTime.Now,
                hierarchy.Data));
    }

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
