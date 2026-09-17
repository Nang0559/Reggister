using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



    namespace FVN_REGISTER.Infrastructure.Services.Leaves
    {
    public class LeaveTypeManagementService
    : BaseService<LeaveTypeManagementService>, ILeaveTypeManagementService
    {
        private readonly IUnitOfWork _uow;

        public LeaveTypeManagementService(
            IUnitOfWork uow,
            ILogger<LeaveTypeManagementService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            // Không còn IHrmSyncEngine<...> — sync đã tách hẳn sang LeaveTypeStagingImporter + LeaveTypeHrmSyncJob
        }

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<ServiceResult<List<LeaveTypeDto>>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE_TYPE] GetAll");

                var data = await _uow.Repository<F03LeaveType>().Query()
                    .AsNoTracking()
                    .OrderBy(x => !x.IsActive)
                    .ThenBy(x => x.LeaveTypeName)
                    .ToListAsync(ct);

                return ServiceResult<List<LeaveTypeDto>>.Ok(data.Select(x => x.ToDto()).ToList());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE_TYPE] GetAll error");
                return ServiceResult<List<LeaveTypeDto>>.Fail("Lỗi tải danh sách hình thức nghỉ.");
            }
        }

        // ── GET FILTERED ─────────────────────────────────────────
        public async Task<ServiceResult<List<LeaveTypeDto>>> GetFilteredAsync(
            bool? tinhPhep, bool? isActive, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE_TYPE] GetFiltered tinhPhep={T} isActive={A}", tinhPhep, isActive);

                var query = _uow.Repository<F03LeaveType>().Query().AsNoTracking();

                if (tinhPhep.HasValue)
                    query = query.Where(x => x.IsCountedAsLeave == tinhPhep.Value);

                if (isActive.HasValue)
                    query = query.Where(x => x.IsActive == isActive.Value);

                var data = await query
                    .OrderBy(x => !x.IsActive)
                    .ThenBy(x => x.LeaveTypeName)
                    .ToListAsync(ct);

                return ServiceResult<List<LeaveTypeDto>>.Ok(data.Select(x => x.ToDto()).ToList());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE_TYPE] GetFiltered error");
                return ServiceResult<List<LeaveTypeDto>>.Fail("Lỗi lọc dữ liệu.");
            }
        }

        // ── CREATE ───────────────────────────────────────────────
        public async Task<ServiceResult> CreateAsync(
            LeaveTypeUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE_TYPE] Create: {Code}", model.LeaveTypeCode);

                var exists = await _uow.Repository<F03LeaveType>().Query()
                    .AnyAsync(x => x.LeaveTypeCode == model.LeaveTypeCode, ct);

                if (exists)
                    return ServiceResult.Fail($"Mã hình thức nghỉ '{model.LeaveTypeCode}' đã tồn tại.");

                var entity = new F03LeaveType
                {
                    LeaveTypeCode = model.LeaveTypeCode.Trim(),
                    LeaveTypeName = model.LeaveTypeName.Trim(),
                    LeaveTypeName2 = model.LeaveTypeName2?.Trim(),
                    IsCountedAsLeave = model.TinhPhep,
                    HRMCode = model.HRMCode?.Trim(),
                    IsActive = model.IsActive,
                    CreatedBy = currentUserId,
                    ModifiedBy = currentUserId
                };

                await _uow.Repository<F03LeaveType>().AddAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[LEAVE_TYPE] Created: {Code}", model.LeaveTypeCode);
                return ServiceResult.Ok("Đã thêm hình thức nghỉ thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE_TYPE] Create error: {Code}", model.LeaveTypeCode);
                return ServiceResult.Fail("Lỗi hệ thống khi thêm hình thức nghỉ.");
            }
        }

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<ServiceResult> UpdateAsync(
            LeaveTypeUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE_TYPE] Update: {Id}", model.Id);

                var entity = await _uow.Repository<F03LeaveType>().Query()
                    .FirstOrDefaultAsync(x => x.Id == model.Id, ct);

                if (entity == null)
                    return ServiceResult.Fail("Không tìm thấy hình thức nghỉ.");

                var codeExists = await _uow.Repository<F03LeaveType>().Query()
                    .AnyAsync(x => x.LeaveTypeCode == model.LeaveTypeCode && x.Id != model.Id, ct);

                if (codeExists)
                    return ServiceResult.Fail($"Mã '{model.LeaveTypeCode}' đã được dùng bởi hình thức khác.");

                entity.LeaveTypeCode = model.LeaveTypeCode.Trim();
                entity.LeaveTypeName = model.LeaveTypeName.Trim();
                entity.LeaveTypeName2 = model.LeaveTypeName2?.Trim();
                entity.IsCountedAsLeave = model.TinhPhep;
                entity.HRMCode = model.HRMCode?.Trim();
                entity.IsActive = model.IsActive;
                entity.ModifiedBy = currentUserId;

                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[LEAVE_TYPE] Updated: {Id}", model.Id);
                return ServiceResult.Ok("Đã cập nhật hình thức nghỉ.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE_TYPE] Update error: {Id}", model.Id);
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật.");
            }
        }

        // ── TOGGLE ───────────────────────────────────────────────
        public async Task<ServiceResult> ToggleAsync(
            int id, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03LeaveType>().Query()
                    .FirstOrDefaultAsync(x => x.Id == id, ct);

                if (entity == null)
                    return ServiceResult.Fail("Không tìm thấy hình thức nghỉ.");

                entity.IsActive = !entity.IsActive;
                entity.ModifiedBy = currentUserId;

                await _uow.SaveChangesAsync(ct);

                var action = entity.IsActive==true ? "kích hoạt" : "tạm dừng";
                Logger.LogInfoIf(Debug, "[LEAVE_TYPE] Toggled {Id} -> {Action}", id, action);

                return ServiceResult.Ok($"Đã {action} hình thức nghỉ.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE_TYPE] Toggle error: {Id}", id);
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật trạng thái.");
            }
        }

        // ── DELETE ───────────────────────────────────────────────
        public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LEAVE_TYPE] Delete: {Id}", id);

                var leaveTypeCode = await _uow.Repository<F03LeaveType>().Query()
                    .Where(lt => lt.Id == id)
                    .Select(lt => lt.LeaveTypeCode)
                    .FirstOrDefaultAsync(ct);

                if (leaveTypeCode == null)
                    return ServiceResult.Fail("Không tìm thấy hình thức nghỉ.");

                var inUse = await _uow.Repository<F03LeaveDay>().Query()
                    .AnyAsync(x => x.LeaveTypeCode == leaveTypeCode, ct);

                if (inUse)
                    return ServiceResult.Fail(
                        "Không thể xóa: hình thức nghỉ này đã có đơn nghỉ liên kết. " +
                        "Hãy dùng 'Tạm dừng' thay vì xóa.");

                var entity = await _uow.Repository<F03LeaveType>().Query()
                    .FirstOrDefaultAsync(x => x.Id == id, ct);

                _uow.Repository<F03LeaveType>().Remove(entity!);
                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[LEAVE_TYPE] Deleted: {Id}", id);
                return ServiceResult.Ok("Đã xóa hình thức nghỉ.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LEAVE_TYPE] Delete error: {Id}", id);
                return ServiceResult.Fail("Lỗi hệ thống khi xóa.");
            }
        }
    }
}

