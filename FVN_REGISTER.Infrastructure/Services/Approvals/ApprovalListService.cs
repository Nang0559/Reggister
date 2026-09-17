using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Interfaces;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    /// <summary>
    /// Logic pending-list dùng chung cho mọi domain (Leave, OT, ...).
    /// Domain cụ thể truyền vào 1 IApprovalListDataSource để biết cách load dữ liệu hiển thị.
    /// </summary>
    public class ApprovalListService<TRow> : BaseService<ApprovalListService<TRow>>
        where TRow : IPendingRequestRow, IHasAttachments
    {
        private readonly IUnitOfWork _uow;
        private readonly IApprovalListDataSource<TRow> _dataSource;
        private readonly IAttachmentService _attachmentService;

        public ApprovalListService(
            IUnitOfWork uow,
            IApprovalListDataSource<TRow> dataSource,
            IAttachmentService attachmentService,
            ILogger<ApprovalListService<TRow>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _dataSource = dataSource;
            _attachmentService = attachmentService;
        }

        /// <summary>Đếm pending theo từng Level (dùng cho widget tổng quan, không load full data).</summary>
        public async Task<List<PendingApprovalSummaryDto>> GetPendingSummaryAsync(
            string approverEmail, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(approverEmail)) return new();

            Logger.LogDebugIf(Debug,
                "[APPROVAL-LIST] GetPendingSummary {Type} - {Email}",
                _dataSource.RequestType, approverEmail);

            var grouped = await _uow.Repository<F03ApprovalStep>()
                .Query()
                .AsNoTracking()
                .Where(s =>
                    s.RequestType == _dataSource.RequestType &&
                    s.ApproverEmail == approverEmail &&
                    s.Approved == null &&
                    s.Required == true)
                .GroupBy(s => s.Level)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return grouped
                .Where(g => g.Count > 0)
                .OrderBy(g => g.Level)
                .Select(g => new PendingApprovalSummaryDto
                {
                    ApproverLevel = g.Level,
                    Count = g.Count
                })
                .ToList();
        }

        /// <summary>Lấy đầy đủ danh sách request đang chờ, gom theo Level (dùng cho trang chi tiết).</summary>
        public async Task<List<PendingApprovalGroupDto>> GetPendingDetailsAsync(
            string approverEmail, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(approverEmail)) return new();

            var pendingSteps = await _uow.Repository<F03ApprovalStep>()
                .Query()
                .AsNoTracking()
                .Where(s =>
                    s.RequestType == _dataSource.RequestType &&
                    s.ApproverEmail == approverEmail &&
                    s.Approved == null &&
                    s.Required == true)
                .ToListAsync(ct);

            if (pendingSteps.Count == 0) return new();

            var requestIds = pendingSteps.Select(s => s.RequestId).Distinct().ToList();

            var rows = await _dataSource.GetActiveRequestsByIdsAsync(requestIds, ct);
            await _attachmentService.EnrichAsync(rows, _dataSource.RequestType, ct);

            Logger.LogInfoIf(Debug,
                "[APPROVAL-LIST] {Type} pending: {Count} requests",
                _dataSource.RequestType, rows.Count);

            return pendingSteps
                .GroupBy(s => s.Level)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var idsAtLevel = g.Select(s => s.RequestId).ToHashSet();
                    var items = rows
                        .Where(r => idsAtLevel.Contains(r.RequestId))
                        .Select(r => _dataSource.ToPendingItem(r, canApprove: true))
                        .ToList();

                    return new PendingApprovalGroupDto
                    {
                        ApproverLevel = g.Key,
                        Count = items.Count,
                        Requests = items
                    };
                })
                .Where(g => g.Count > 0)
                .ToList();
        }
    }
}
