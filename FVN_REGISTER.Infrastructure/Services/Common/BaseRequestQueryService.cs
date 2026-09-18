using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Requests;

using FVN_REGISTER.Core.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Common
{
    public abstract class BaseRequestQueryService<TSummary, TBalance, TDetail, TDto>
        : IRequestQueryService<TSummary, TBalance, TDto>
        where TSummary : class
        where TDto : BaseRequestDto<TDetail>
    {
        protected readonly IUnitOfWork Uow;
        protected readonly IApprovalHistoryService HistoryService;
        protected readonly IAttachmentService AttachmentService;
        protected readonly ICurrentUserService CurrentUserService;

        protected abstract RequestModule Module { get; }

        protected BaseRequestQueryService(
            IUnitOfWork uow,
            IApprovalHistoryService historyService,
            IAttachmentService attachmentService,
            ICurrentUserService currentUserService)
        {
            Uow = uow;
            HistoryService = historyService;
            AttachmentService = attachmentService;
            CurrentUserService = currentUserService;
        }

        public virtual async Task<ServiceResult<TDto>> GetFullDetailsAsync(
            int requestId, CancellationToken ct = default)
        {
            var header = await GetHeaderByIdAsync(requestId, ct);
            if (header == null)
                return ServiceResult<TDto>.Fail("Không tìm thấy đơn.");

            var details = await GetDetailsByIdAsync(requestId, ct);
            header.Details = details;

            var snapshot = await Uow.Repository<F03ApprovalSnapshot>().Query()
                .AsNoTracking()
                .Include(s => s.Steps)
                .FirstOrDefaultAsync(
                    s => s.RequestId == requestId && s.RequestType == Module,
                    ct);

            if (snapshot != null)
            {
                var histories = await HistoryService.GetHistoryAsync(requestId, Module, ct);
                var currentUser = CurrentUserService.GetCurrentUser();

                var calculated = ApprovalStepMapper.MapToCalculatedList(
                    snapshot.Steps, histories, currentUser);

                header.ApprovalSteps = calculated;
                header.RequestStatus = ApprovalStepMapper.ComputeOverallStatus(calculated);
            }

            await AttachmentService.EnrichAsync(new[] { header }, Module, ct);

            return ServiceResult<TDto>.Ok(header);
        }

        protected abstract Task<TDto?> GetHeaderByIdAsync(
            int requestId, CancellationToken ct);

        protected abstract Task<List<TDetail>> GetDetailsByIdAsync(
            int requestId, CancellationToken ct);

        public abstract Task<List<WidgetCounterDto>> GetMyWidgetsAsync(
            string employeeCode, CancellationToken ct = default);

        public abstract Task<List<TSummary>> GetRecentSummaryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default);

        public abstract Task<TBalance> GetSimpleBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default);

        public abstract Task<PaginationResult<TSummary>> GetPagedAsync(
            string? deptCode,
            ApprovalStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize,
            CancellationToken ct = default);

        public abstract Task<List<TDto>> GetDeptByDateAsync(
            string deptCode,
            DateTime date,
            CancellationToken ct = default);
    }
}
