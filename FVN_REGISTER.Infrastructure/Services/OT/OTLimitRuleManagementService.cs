using FVN_REGISTER.Application.Interfaces.OTLimitRules;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Core.Constants;

using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.OT;

public sealed class OTLimitRuleManagementService : BaseService<OTLimitRuleManagementService>, IOTLimitRuleManagementService
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
            var rows = await _uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .OrderBy(x => x.LimitType)
                .ThenBy(x => x.ScopeType)
                .ThenBy(x => x.ScopeCode)
                .ThenBy(x => x.EmployeeCode)
                .ThenBy(x => x.DeptCode)
                .ThenBy(x => x.PositionCode)
                .ToListAsync(ct);

            return ServiceResult<List<OTLimitRuleDto>>.Ok(rows.Select(x => x.ToDto()).ToList());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OTLimitRule] GetAll failed");
            return ServiceResult<List<OTLimitRuleDto>>.Fail("Không thể tải danh sách hạn mức OT.");
        }
    }

    public async Task<ServiceResult<OTLimitRuleDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            return entity == null
                ? ServiceResult<OTLimitRuleDto>.Fail("Không tìm thấy quy tắc hạn mức OT.")
                : ServiceResult<OTLimitRuleDto>.Ok(entity.ToDto());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OTLimitRule] GetById failed: {Id}", id);
            return ServiceResult<OTLimitRuleDto>.Fail("Không thể tải quy tắc hạn mức OT.");
        }
    }

    public async Task<ServiceResult> CreateAsync(
        OTLimitRuleUpsertDto model,
        int currentUserId,
        CancellationToken ct = default)
    {
        try
        {
            var validation = await ValidateModelAsync(model, null, ct);
            if (validation != null) return ServiceResult.Fail(validation);

            var duplicate = await HasActiveDuplicateAsync(model, null, ct);
            if (duplicate) return ServiceResult.Fail("Đã tồn tại hạn mức OT đang hoạt động cho cùng phạm vi và loại giới hạn.");

            var entity = model.ToEntity(currentUserId);
            await _uow.Repository<F03OTLimitRule>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return ServiceResult.Ok("Đã thêm quy tắc hạn mức OT.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OTLimitRule] Create failed");
            return ServiceResult.Fail("Không thể thêm quy tắc hạn mức OT.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(
        OTLimitRuleUpsertDto model,
        int currentUserId,
        CancellationToken ct = default)
    {
        try
        {
            if (model.Id <= 0) return ServiceResult.Fail("ID quy tắc không hợp lệ.");

            var entity = await _uow.Repository<F03OTLimitRule>().Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id, ct);

            if (entity == null) return ServiceResult.Fail("Không tìm thấy quy tắc hạn mức OT.");

            var validation = await ValidateModelAsync(model, entity.Id, ct);
            if (validation != null) return ServiceResult.Fail(validation);

            if (await HasActiveDuplicateAsync(model, entity.Id, ct))
                return ServiceResult.Fail("Đã tồn tại hạn mức OT đang hoạt động cho cùng phạm vi và loại giới hạn.");

            model.ApplyTo(entity, currentUserId);
            entity.LastModifiedSource = SyncSourceTags.Manual;
            await _uow.SaveChangesAsync(ct);
            return ServiceResult.Ok("Đã cập nhật quy tắc hạn mức OT.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OTLimitRule] Update failed: {Id}", model.Id);
            return ServiceResult.Fail("Không thể cập nhật quy tắc hạn mức OT.");
        }
    }

    public async Task<ServiceResult> ToggleActiveAsync(
        int id,
        int currentUserId,
        CancellationToken ct = default)
    {
        try
        {
            var entity = await _uow.Repository<F03OTLimitRule>().Query()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null) return ServiceResult.Fail("Không tìm thấy quy tắc hạn mức OT.");

            if (entity.LastModifiedSource == SyncSourceTags.Hrm)
                return ServiceResult.Fail("Quy tắc được đồng bộ từ HRM, không thể thay đổi trạng thái tại đây.");

            entity.IsActive = entity.IsActive != true;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
            entity.LastModifiedSource = SyncSourceTags.Manual;
            await _uow.SaveChangesAsync(ct);

            return ServiceResult.Ok(entity.IsActive == true ? "Đã kích hoạt hạn mức OT." : "Đã tạm dừng hạn mức OT.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OTLimitRule] Toggle failed: {Id}", id);
            return ServiceResult.Fail("Không thể cập nhật trạng thái hạn mức OT.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _uow.Repository<F03OTLimitRule>().Query()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null) return ServiceResult.Fail("Không tìm thấy quy tắc hạn mức OT.");
            if (entity.LastModifiedSource == SyncSourceTags.Hrm)
                return ServiceResult.Fail("Quy tắc được đồng bộ từ HRM, không thể xóa.");

            _uow.Repository<F03OTLimitRule>().Remove(entity);
            await _uow.SaveChangesAsync(ct);
            return ServiceResult.Ok("Đã xóa quy tắc hạn mức OT.");
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "[OTLimitRule] Delete DB failed: {Id}", id);
            return ServiceResult.Fail("Không thể xóa quy tắc hạn mức OT vì dữ liệu đang được tham chiếu.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OTLimitRule] Delete failed: {Id}", id);
            return ServiceResult.Fail("Không thể xóa quy tắc hạn mức OT.");
        }
    }

    public async Task<List<OTLimitRuleDto>> GetApplicableRulesAsync(
        string deptCode,
        string positionCode,
        OTLimitType limitType,
        CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03OTLimitRule>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.LimitType == limitType)
            .ToListAsync(ct);

        return rows
            .Where(x =>
                x.ScopeType == OTLimitScopeType.Employee
                && (string.IsNullOrWhiteSpace(x.EmployeeCode)
                    || x.EmployeeCode == null)
                && (x.DeptCode == null || x.DeptCode == deptCode)
                && (x.PositionCode == null || x.PositionCode == positionCode))
            .OrderBy(x => x.EmployeeCode == null ? 2 : 1)
            .ThenByDescending(x => x.DeptCode != null && x.PositionCode != null)
            .ThenByDescending(x => x.DeptCode != null)
            .ThenByDescending(x => x.PositionCode != null)
            .Select(x => x.ToDto())
            .ToList();
    }

    private async Task<string?> ValidateModelAsync(
        OTLimitRuleUpsertDto model,
        int? currentId,
        CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(OTLimitType), model.LimitType))
            return "Loại giới hạn OT không hợp lệ.";

        if (!Enum.IsDefined(typeof(OTLimitScopeType), model.ScopeType))
            return "Phạm vi hạn mức OT không hợp lệ.";

        if (model.LimitHours <= 0 && model.LimitValue > 0)
            model.LimitHours = model.LimitValue;

        if (model.LimitHours <= 0 || model.LimitHours > 999)
            return "Số giờ giới hạn phải nằm trong khoảng 0.1 đến 999.";

        model.ScopeCode = Clean(model.ScopeCode);
        model.EmployeeCode = Clean(model.EmployeeCode);
        model.DeptCode = Clean(model.DeptCode);
        model.PositionCode = Clean(model.PositionCode);
        model.Description = Clean(model.Description);

        switch (model.ScopeType)
        {
            case OTLimitScopeType.Employee:
                if (!string.IsNullOrWhiteSpace(model.EmployeeCode))
                {
                    var employeeExists = await _uow.Repository<F03Employee>().Query()
                        .AnyAsync(x => x.IsActive == true && x.EmployeeCode == model.EmployeeCode, ct);
                    if (!employeeExists) return $"Nhân viên '{model.EmployeeCode}' không tồn tại hoặc đã ngừng hoạt động.";
                }
                model.ScopeCode = null;
                break;

            case OTLimitScopeType.Department:
                if (string.IsNullOrWhiteSpace(model.ScopeCode))
                    return "Phạm vi Phòng ban phải có mã phòng ban.";

                var deptExists = await _uow.Repository<F03Department>().Query()
                    .AnyAsync(x => x.IsActive == true && x.DeptCode == model.ScopeCode, ct);
                if (!deptExists) return $"Phòng ban '{model.ScopeCode}' không tồn tại hoặc đã ngừng hoạt động.";

                model.EmployeeCode = null;
                model.DeptCode = model.ScopeCode;
                model.PositionCode = null;
                break;

            case OTLimitScopeType.Block:
                if (string.IsNullOrWhiteSpace(model.ScopeCode))
                    return "Phạm vi Khối phải có mã khối.";

                var blockExists = await _uow.Repository<F03Department>().Query()
                    .AnyAsync(x => x.IsActive == true && x.BlockCode == model.ScopeCode, ct);
                if (!blockExists) return $"Khối '{model.ScopeCode}' không tồn tại hoặc chưa được cấu hình.";

                model.EmployeeCode = null;
                model.DeptCode = null;
                model.PositionCode = null;
                break;
        }

        return null;
    }

    private async Task<bool> HasActiveDuplicateAsync(
        OTLimitRuleUpsertDto model,
        int? currentId,
        CancellationToken ct)
    {
        var query = _uow.Repository<F03OTLimitRule>().Query()
            .Where(x => x.IsActive == true
                && x.LimitType == model.LimitType
                && x.ScopeType == model.ScopeType
                && x.ScopeCode == model.ScopeCode
                && x.EmployeeCode == model.EmployeeCode
                && x.DeptCode == model.DeptCode
                && x.PositionCode == model.PositionCode);

        if (currentId.HasValue) query = query.Where(x => x.Id != currentId.Value);
        return await query.AnyAsync(ct);
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
