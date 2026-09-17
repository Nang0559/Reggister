using FVN_REGISTER.Application.Interfaces.OTLimitRules;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Application.Maps;

namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTLimitRuleManagementService
         : BaseService<OTLimitRuleManagementService>, IOTLimitRuleManagementService
    {
        private readonly IUnitOfWork _uow;

        public OTLimitRuleManagementService(
            IUnitOfWork uow,
            ILogger<OTLimitRuleManagementService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        public async Task<ServiceResult<List<OTLimitRuleDto>>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var data = await _uow.Repository<F03OTLimitRule>().Query()
                    .AsNoTracking()
                    .OrderBy(x => x.LimitType)
                    .ThenBy(x => x.DeptCode)
                    .ThenBy(x => x.PositionCode)
                    .ToListAsync(ct);

                return ServiceResult<List<OTLimitRuleDto>>.Ok(data.Select(x => x.ToDto()).ToList());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OTLimitRule] GetAll error");
                return ServiceResult<List<OTLimitRuleDto>>.Fail("Lỗi tải danh sách hạn mức OT.");
            }
        }

        public async Task<ServiceResult<OTLimitRuleDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03OTLimitRule>().GetByIdAsync(id, ct);
                if (entity == null) return ServiceResult<OTLimitRuleDto>.Fail("Không tìm thấy hạn mức OT.");
                return ServiceResult<OTLimitRuleDto>.Ok(entity.ToDto());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OTLimitRule] GetById error: {Id}", id);
                return ServiceResult<OTLimitRuleDto>.Fail("Lỗi tải thông tin hạn mức OT.");
            }
        }

        public async Task<ServiceResult> CreateAsync(
            OTLimitRuleUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                if (await IsDuplicateScopeAsync(model.LimitType, model.DeptCode, model.PositionCode, excludeId: null, ct))
                    return ServiceResult.Fail(
                        "Đã tồn tại rule cùng loại hạn mức + cùng phạm vi Phòng ban/Vị trí. Vui lòng sửa rule đó thay vì tạo mới.");

                var entity = model.ToEntity(currentUserId);
                await _uow.Repository<F03OTLimitRule>().AddAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[OTLimitRule] Created: {Type} Dept={Dept} Pos={Pos}",
                    model.LimitType, model.DeptCode, model.PositionCode);

                return ServiceResult.Ok("Đã thêm hạn mức OT.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OTLimitRule] Create error");
                return ServiceResult.Fail("Lỗi hệ thống khi thêm hạn mức OT.");
            }
        }

        public async Task<ServiceResult> UpdateAsync(
            OTLimitRuleUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03OTLimitRule>().GetByIdAsync(model.Id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy hạn mức OT.");

                if (await IsDuplicateScopeAsync(model.LimitType, model.DeptCode, model.PositionCode, excludeId: model.Id, ct))
                    return ServiceResult.Fail("Phạm vi này đã có rule khác cùng loại hạn mức.");

                model.ApplyTo(entity, currentUserId);
                entity.ModifiedBy = currentUserId;
                entity.ModifiedAt = DateTime.Now;

                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[OTLimitRule] Updated: {Id}", model.Id);
                return ServiceResult.Ok("Đã cập nhật hạn mức OT.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OTLimitRule] Update error");
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật hạn mức OT.");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03OTLimitRule>().GetByIdAsync(id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy hạn mức OT.");

                _uow.Repository<F03OTLimitRule>().Remove(entity);
                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[OTLimitRule] Deleted: {Id}", id);
                return ServiceResult.Ok("Đã xóa hạn mức OT.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OTLimitRule] Delete error: {Id}", id);
                return ServiceResult.Fail("Lỗi hệ thống khi xóa hạn mức OT.");
            }
        }

        /// <summary>
        /// Trả về các rule khớp với nhân viên (Dept/Position), sắp theo độ ưu tiên
        /// GIẢM DẦN (rule cụ thể nhất đứng đầu): Dept+Position > Dept-only > Position-only > toàn công ty.
        /// Caller thường chỉ cần lấy phần tử đầu tiên (rule áp dụng thực tế),
        /// nhưng trả cả list để UI có thể hiện "rule nào đang override rule nào" nếu cần.
        /// </summary>
        public async Task<List<OTLimitRuleDto>> GetApplicableRulesAsync(
            string? deptCode, string? positionCode, OTLimitType limitType, CancellationToken ct = default)
        {
            var candidates = await _uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(x => x.LimitType == limitType)
                .Where(x =>
                    (x.DeptCode == null && x.PositionCode == null) ||                              // toàn công ty
                    (x.DeptCode == deptCode && x.PositionCode == null) ||                           // theo Dept
                    (x.PositionCode == positionCode && x.DeptCode == null) ||                       // theo Position
                    (x.DeptCode == deptCode && x.PositionCode == positionCode))                     // theo cả 2
                .ToListAsync(ct);

            return candidates
                .Select(x => new { Entity = x, Priority = GetScopePriority(x.DeptCode, x.PositionCode) })
                .OrderBy(x => x.Priority) // 1 = cụ thể nhất
                .Select(x => x.Entity.ToDto())
                .ToList();
        }

        // ===== Helpers =====

        private static int GetScopePriority(string? deptCode, string? positionCode)
        {
            if (deptCode != null && positionCode != null) return 1; // cụ thể nhất
            if (deptCode != null) return 2;
            if (positionCode != null) return 3;
            return 4; // toàn công ty — mặc định, ưu tiên thấp nhất
        }

        private async Task<bool> IsDuplicateScopeAsync(
            OTLimitType limitType, string? deptCode, string? positionCode, int? excludeId, CancellationToken ct)
        {
            var query = _uow.Repository<F03OTLimitRule>().Query()
                .Where(x => x.LimitType == limitType
                         && x.DeptCode == deptCode
                         && x.PositionCode == positionCode);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }
    }
}
