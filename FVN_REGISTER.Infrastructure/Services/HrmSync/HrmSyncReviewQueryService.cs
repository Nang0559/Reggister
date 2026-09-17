using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.HrmSync;

using FVN_REGISTER.Core.Repositories;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync
{
    public class HrmSyncReviewQueryService : BaseService<HrmSyncReviewQueryService>, IHrmSyncReviewQueryService
    {
        private readonly IUnitOfWork _uow;

        public HrmSyncReviewQueryService(
            IUnitOfWork uow,
            ILogger<HrmSyncReviewQueryService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        public async Task<ServiceResult<List<SyncReviewFlagDto>>> GetUnresolvedAsync(
            string? entityType = null, CancellationToken ct = default)
        {
            try
            {
                var query = _uow.Repository<F03SyncReviewFlag>().Query()
                    .Where(x => !x.IsResolved);

                if (!string.IsNullOrWhiteSpace(entityType))
                    query = query.Where(x => x.EntityType == entityType);

                var list = await query
                    .OrderByDescending(x => x.DetectedAt)
                    .Select(x => new SyncReviewFlagDto
                    {
                        Id = x.Id,
                        EntityType = x.EntityType,
                        EntityKey = x.EntityKey,
                        FlagType = x.FlagType,
                        Message = x.Message,
                        DetectedAt = x.DetectedAt,
                        IsResolved = x.IsResolved,
                        ResolvedAt = x.ResolvedAt,
                        ResolvedBy = x.ResolvedBy
                    })
                    .ToListAsync(ct);

                return ServiceResult<List<SyncReviewFlagDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                return InternalError<List<SyncReviewFlagDto>>(ex, "Lỗi hệ thống khi tải cảnh báo đồng bộ.");
            }
        }

        public async Task<ServiceResult<int>> CountUnresolvedAsync(CancellationToken ct = default)
        {
            try
            {
                var count = await _uow.Repository<F03SyncReviewFlag>().Query()
                    .CountAsync(x => !x.IsResolved, ct);

                return ServiceResult<int>.Ok(count);
            }
            catch (Exception ex)
            {
                return InternalError<int>(ex, "Lỗi hệ thống khi đếm cảnh báo đồng bộ.");
            }
        }

        public async Task<ServiceResult> ResolveAsync(
            int flagId, string resolvedBy, CancellationToken ct = default)
        {
            try
            {
                var flag = await _uow.Repository<F03SyncReviewFlag>().GetByIdAsync(flagId, ct);
                if (flag == null)
                    return ServiceResult.Fail("Không tìm thấy cảnh báo.");

                if (flag.IsResolved)
                    return ServiceResult.Ok("Cảnh báo đã được xử lý trước đó.");

                flag.IsResolved = true;
                flag.ResolvedAt = DateTime.Now;
                flag.ResolvedBy = resolvedBy;

                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[SYNC-REVIEW] Resolved FlagId={Id} By={User}", flagId, resolvedBy);

                return ServiceResult.Ok("Đã đánh dấu xử lý xong.");
            }
            catch (Exception ex)
            {
                return InternalError(ex, "Lỗi hệ thống khi xử lý cảnh báo.");
            }
        }

        public async Task<ServiceResult> ResolveByEntityAsync(
            string entityType, string entityKey, string resolvedBy, CancellationToken ct = default)
        {
            try
            {
                var flags = await _uow.Repository<F03SyncReviewFlag>().Query()
                    .Where(x => x.EntityType == entityType && x.EntityKey == entityKey && !x.IsResolved)
                    .ToListAsync(ct);

                if (flags.Count == 0)
                    return ServiceResult.Ok("Không có cảnh báo nào cần xử lý.");

                var now = DateTime.Now;
                foreach (var f in flags)
                {
                    f.IsResolved = true;
                    f.ResolvedAt = now;
                    f.ResolvedBy = resolvedBy;
                }

                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[SYNC-REVIEW] Resolved {Count} flags for {Type}/{Key} By={User}",
                    flags.Count, entityType, entityKey, resolvedBy);

                return ServiceResult.Ok($"Đã xử lý {flags.Count} cảnh báo.");
            }
            catch (Exception ex)
            {
                return InternalError(ex, "Lỗi hệ thống khi xử lý cảnh báo hàng loạt.");
            }
        }
    }
}
