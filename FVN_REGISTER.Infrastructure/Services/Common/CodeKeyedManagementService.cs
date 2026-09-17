using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FVN_REGISTER.Infrastructure.Services.Common
{
    /// <summary>
    /// Base cho các ManagementService quản lý entity có khóa nghiệp vụ dạng string code
    /// (LeaveTypeCode, PositionCode, DeptCode...). Xử lý sẵn CRUD chuẩn: GetAll, GetFiltered,
    /// GetById, Create, Update, Toggle, Delete (kèm check trùng code + check in-use).
    /// Derived class chỉ cần khai báo mapping + rule đặc thù qua các abstract/virtual member.
    /// </summary>
    /// <summary>
    /// Base CRUD cho entity có khóa nghiệp vụ dạng code (string) và một TKey dùng để
    /// tra cứu/xóa/toggle (TKey có thể là int Id hoặc chính string code, tùy entity).
    /// </summary>
    public abstract class CodeKeyedManagementService<TEntity, TKey, TDto, TUpsertDto>
        : BaseService<CodeKeyedManagementService<TEntity, TKey, TDto, TUpsertDto>>
        where TEntity : BaseAuditEntity
    {
        protected readonly IUnitOfWork Uow;

        protected CodeKeyedManagementService(
            IUnitOfWork uow,
            ILogger<CodeKeyedManagementService<TEntity, TKey, TDto, TUpsertDto>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            Uow = uow;
        }

        protected abstract string EntityLabel { get; }
        protected abstract string GetCode(TEntity entity);
        protected abstract bool GetIsActive(TEntity entity);
        protected abstract void SetIsActive(TEntity entity, bool value);
        protected abstract void TouchModified(TEntity entity, int currentUserId);
        protected abstract TKey GetUpsertKey(TUpsertDto model);
        protected abstract string GetUpsertCode(TUpsertDto model);
        protected abstract TDto ToDto(TEntity entity);
        protected abstract TEntity ToNewEntity(TUpsertDto model, int currentUserId);
        protected abstract void ApplyUpsert(TEntity entity, TUpsertDto model, int currentUserId);
        protected abstract Expression<Func<TEntity, bool>> KeyEqualsExpr(TKey key);
        protected abstract Expression<Func<TEntity, bool>> KeyNotEqualsExpr(TKey key);
        protected abstract Expression<Func<TEntity, bool>> CodeEqualsExpr(string code);
        protected abstract IQueryable<TEntity> ApplyActiveFilter(IQueryable<TEntity> query, bool isActive);
        protected abstract Task<bool> IsInUseAsync(TEntity entity, CancellationToken ct);

        protected virtual IOrderedQueryable<TEntity> ApplyDefaultOrder(IQueryable<TEntity> query)
            => query.OrderBy(x => GetCode(x));

        public async Task<ServiceResult<List<TDto>>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[{Label}] GetAll", EntityLabel);
                var data = await ApplyDefaultOrder(Uow.Repository<TEntity>().Query().AsNoTracking()).ToListAsync(ct);
                return ServiceResult<List<TDto>>.Ok(data.Select(ToDto).ToList());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] GetAll error", EntityLabel);
                return ServiceResult<List<TDto>>.Fail($"Lỗi tải danh sách {EntityLabel}.");
            }
        }

        protected async Task<ServiceResult<List<TDto>>> GetFilteredCoreAsync(
            bool? isActive,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? extraFilter,
            CancellationToken ct)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[{Label}] GetFiltered isActive={A}", EntityLabel, isActive);
                var query = Uow.Repository<TEntity>().Query().AsNoTracking();
                if (isActive.HasValue) query = ApplyActiveFilter(query, isActive.Value);
                if (extraFilter != null) query = extraFilter(query);
                var data = await ApplyDefaultOrder(query).ToListAsync(ct);
                return ServiceResult<List<TDto>>.Ok(data.Select(ToDto).ToList());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] GetFiltered error", EntityLabel);
                return ServiceResult<List<TDto>>.Fail("Lỗi lọc dữ liệu.");
            }
        }

        public async Task<ServiceResult<TDto>> GetByIdAsync(TKey key, CancellationToken ct = default)
        {
            try
            {
                var entity = await Uow.Repository<TEntity>().Query().AsNoTracking()
                    .FirstOrDefaultAsync(KeyEqualsExpr(key), ct);
                if (entity == null) return ServiceResult<TDto>.Fail($"Không tìm thấy {EntityLabel}.");
                return ServiceResult<TDto>.Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] GetById error", EntityLabel);
                return ServiceResult<TDto>.Fail($"Lỗi tải thông tin {EntityLabel}.");
            }
        }

        public async Task<ServiceResult> CreateAsync(TUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var code = GetUpsertCode(model);
                Logger.LogDebugIf(Debug, "[{Label}] Create: {Code}", EntityLabel, code);
                var exists = await Uow.Repository<TEntity>().Query().AnyAsync(CodeEqualsExpr(code), ct);
                if (exists) return ServiceResult.Fail($"Mã '{code}' đã tồn tại.");
                var entity = ToNewEntity(model, currentUserId);
                await Uow.Repository<TEntity>().AddAsync(entity, ct);
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Label}] Created: {Code}", EntityLabel, code);
                return ServiceResult.Ok($"Đã thêm {EntityLabel} thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] Create error", EntityLabel);
                return ServiceResult.Fail($"Lỗi hệ thống khi thêm {EntityLabel}.");
            }
        }

        public async Task<ServiceResult> UpdateAsync(TUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var key = GetUpsertKey(model);
                var code = GetUpsertCode(model);
                Logger.LogDebugIf(Debug, "[{Label}] Update: {Key}", EntityLabel, key);
                var entity = await Uow.Repository<TEntity>().Query().FirstOrDefaultAsync(KeyEqualsExpr(key), ct);
                if (entity == null) return ServiceResult.Fail($"Không tìm thấy {EntityLabel}.");
                var currentCode = GetCode(entity);
                var codeExists = await Uow.Repository<TEntity>().Query()
                    .Where(CodeEqualsExpr(code)).Where(KeyNotEqualsExpr(key)).AnyAsync(ct);
                if (codeExists) return ServiceResult.Fail($"Mã '{code}' đã được dùng bởi {EntityLabel} khác.");
                bool codeChanged = currentCode != code;
                if (codeChanged && await IsInUseAsync(entity, ct))
                    return ServiceResult.Fail($"Không thể đổi mã khi {EntityLabel} đang được liên kết ở nơi khác.");
                ApplyUpsert(entity, model, currentUserId);
                entity.LastModifiedSource = SyncSourceTags.Manual;
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Label}] Updated: {Key}", EntityLabel, key);
                return ServiceResult.Ok($"Đã cập nhật {EntityLabel}.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] Update error", EntityLabel);
                return ServiceResult.Fail($"Lỗi hệ thống khi cập nhật {EntityLabel}.");
            }
        }

        public async Task<ServiceResult> ToggleActiveAsync(TKey key, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var entity = await Uow.Repository<TEntity>().Query().FirstOrDefaultAsync(KeyEqualsExpr(key), ct);
                if (entity == null) return ServiceResult.Fail($"Không tìm thấy {EntityLabel}.");
                var newValue = !GetIsActive(entity);
                SetIsActive(entity, newValue);
                TouchModified(entity, currentUserId);
                entity.LastModifiedSource = SyncSourceTags.Manual;
                await Uow.SaveChangesAsync(ct);
                var action = newValue ? "kích hoạt" : "tạm dừng";
                Logger.LogInfoIf(Debug, "[{Label}] Toggled {Key} -> {Action}", EntityLabel, key, action);
                return ServiceResult.Ok($"Đã {action} {EntityLabel}.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] Toggle error", EntityLabel);
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật trạng thái.");
            }
        }

        public async Task<ServiceResult> DeleteAsync(TKey key, CancellationToken ct = default)
        {
            try
            {
                var entity = await Uow.Repository<TEntity>().Query().FirstOrDefaultAsync(KeyEqualsExpr(key), ct);
                if (entity == null) return ServiceResult.Fail($"Không tìm thấy {EntityLabel}.");
                if (entity.LastModifiedSource == SyncSourceTags.Hrm)
                    return ServiceResult.Fail($"{EntityLabel} này được đồng bộ từ HRM, không thể xóa cứng. Hãy dùng chức năng 'Tạm dừng' thay vì xóa.");
                if (await IsInUseAsync(entity, ct))
                    return ServiceResult.Fail($"Không thể xóa: {EntityLabel} này đang được liên kết. Hãy dùng 'Tạm dừng' thay vì xóa.");
                Uow.Repository<TEntity>().Remove(entity);
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Label}] Deleted: {Key}", EntityLabel, key);
                return ServiceResult.Ok($"Đã xóa {EntityLabel}.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[{Label}] Delete error", EntityLabel);
                return ServiceResult.Fail("Lỗi hệ thống khi xóa.");
            }
        }
    }
}
