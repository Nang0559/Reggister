namespace FVN_REGISTER.Infrastructure.Services.Common
{
    using FVN_REGISTER.Application.Interfaces.Approvals;
    using FVN_REGISTER.Application.Interfaces.Common;
    using FVN_REGISTER.Application.Interfaces.Orchestrators;
    using FVN_REGISTER.Contract.Dtos.Approvals;
    using FVN_REGISTER.Contract.Dtos.Authentication;
    using FVN_REGISTER.Contract.Requests.Leaves;
    using FVN_REGISTER.Contract.Responses;
    using FVN_REGISTER.Core.Configurations;
    using FVN_REGISTER.Core.Constants;
    using FVN_REGISTER.Core.Entities;
    using FVN_REGISTER.Core.Entities.Common;
    using FVN_REGISTER.Core.Enums;
    using FVN_REGISTER.Core.Logging;
    using FVN_REGISTER.Core.Repositories;
    using FVN_REGISTER.Core.Utils;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public abstract class BaseRequestCommandService<TCreateModel, TEntity, TSubject>
        : BaseService<BaseRequestCommandService<TCreateModel, TEntity, TSubject>>,
          IRequestCommandService<TCreateModel>
        where TEntity : BaseRequestEntity
        where TSubject : class, IApprovalSubject
    {
        protected readonly IUnitOfWork Uow;
        protected readonly IApprovalWorkflowOrchestrator<TSubject> Workflow;
        protected abstract RequestModule ModuleKind { get; }

        protected BaseRequestCommandService(
            IUnitOfWork uow,
            IApprovalWorkflowOrchestrator<TSubject> workflow,
            ILogger<BaseRequestCommandService<TCreateModel, TEntity, TSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            Uow = uow;
            Workflow = workflow;
        }

        public abstract Task<ServiceResult<int>> CreateAsync(
            TCreateModel model, UserIdentityDto user, CancellationToken ct = default);

        public virtual async Task<ServiceResult> ApproveAsync(
            List<int> ids, int level, UserIdentityDto user, string? comment, CancellationToken ct = default)
            => await ProcessBulkAsync(ids, level, user, comment, isReject: false, ct);

        public virtual async Task<ServiceResult> RejectAsync(
            List<int> ids, int level, UserIdentityDto user, string comment, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return ServiceResult.Fail("Lý do từ chối không được để trống.");
            return await ProcessBulkAsync(ids, level, user, comment, isReject: true, ct);
        }

        public virtual async Task<ServiceResult> CancelAsync(
            int requestId, string reason, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var repo = Uow.Repository<TEntity>();
                var entity = await repo.GetByIdAsync(requestId, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn.");
                bool isOwner = entity.EmployeeCode == user.EmployeeCode;
                bool isAdmin = IsAdmin(user);
                if (!isOwner && !isAdmin) return ServiceResult.Fail("Không có quyền hủy đơn này.");
                if (IsFinalized(entity) && !isAdmin) return ServiceResult.Fail("Đơn đã xử lý xong, không thể hủy.");
                ApplyCancel(entity, reason, user);
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] Cancelled Id={Id} By={User}", ComponentName, requestId, user.EmployeeCode);
                return ServiceResult.Ok("Đã hủy đơn thành công.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi hủy đơn."); }
        }

        private async Task<ServiceResult> ProcessBulkAsync(
            List<int> ids, int level, UserIdentityDto user, string? comment, bool isReject, CancellationToken ct)
        {
            if (ids == null || ids.Count == 0) return ServiceResult.Fail("Không có đơn nào được chọn.");
            try
            {
                var action = new ApprovalActionDto
                {
                    RequestIds = ids,
                    Kind = ModuleKind,
                    Level = level,
                    IsReject = isReject,
                    Comment = comment,
                    ApproverCode = user.EmployeeCode ?? "",
                    ApproverPermission = user.Permission,
                    ApproverPositionCode = user.PositionCode
                };
                ApprovalActionResult result = isReject
                    ? await Workflow.RejectAsync(action, ct)
                    : await Workflow.ApproveAsync(action, ct);
                Logger.LogInfoIf(Debug, "[{Component}] {Action} bulk Level={Level} {Success}/{Total} thành công",
                    ComponentName, isReject ? "Reject" : "Approve", level, result.SuccessCount, result.TotalCount);
                return result.Success
                    ? ServiceResult.Ok(result.Message)
                    : ServiceResult.Fail(result.Message ?? (isReject ? "Từ chối thất bại." : "Duyệt thất bại."));
            }
            catch (Exception ex)
            {
                return InternalError(ex, isReject ? "Lỗi hệ thống khi từ chối đơn." : "Lỗi hệ thống khi duyệt đơn.");
            }
        }

        public virtual async Task<ServiceResult> AttachFilesAsync(
            AttachFilesCommandDto command, UserIdentityDto user, CancellationToken ct = default)
        {
            if (command.Attachments == null || command.Attachments.Count == 0)
                return ServiceResult.Fail("Không có file nào để đính kèm.");
            try
            {
                var entity = await Uow.Repository<TEntity>().GetByIdAsync(command.Id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy đơn.");
                bool isOwner = entity.EmployeeCode == user.EmployeeCode;
                if (!isOwner && !IsAdmin(user)) return ServiceResult.Fail("Không có quyền đính kèm file cho đơn này.");
                if (IsFinalized(entity) && !IsAdmin(user)) return ServiceResult.Fail("Đơn đã xử lý xong, không thể thêm đính kèm.");

                var attachRepo = Uow.Repository<F03Attachment>();
                foreach (var a in command.Attachments)
                {
                    await attachRepo.AddAsync(new F03Attachment
                    {
                        Module = ModuleKind,
                        RequestId = command.Id,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileExtension = a.FileExtension,
                        FileSize = a.FileSize,
                        IsActive = true,
                        CreatedBy = user.UserId,
                        CreatedAt = DateTime.Now
                    }, ct);
                }
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] Attached {Count} files to Id={Id} | Module={Module}", ComponentName, command.Attachments.Count, command.Id, ModuleKind);
                return ServiceResult.Ok("Đã đính kèm file thành công.");
            }
            catch (Exception ex) { return InternalError(ex, "Lỗi hệ thống khi đính kèm file."); }
        }

        protected virtual bool IsFinalized(TEntity entity)
            => entity.RequestStatus == ApprovalStatus.Approved
            || entity.RequestStatus == ApprovalStatus.Rejected
            || entity.RequestStatus == ApprovalStatus.Cancelled;

        protected virtual void ApplyCancel(TEntity entity, string reason, UserIdentityDto user)
        {
            entity.RequestStatus = ApprovalStatus.Cancelled;
            entity.IsActive = false;
        }

        protected virtual bool IsAdmin(UserIdentityDto user) => user.Permission.IsAdmin();
    }
}
