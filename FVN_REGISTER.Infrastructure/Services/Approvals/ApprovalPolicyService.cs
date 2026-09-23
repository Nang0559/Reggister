using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public sealed class ApprovalPolicyService : IApprovalPolicyService
{
    private readonly IUnitOfWork _uow;

    public ApprovalPolicyService(IUnitOfWork uow) => _uow = uow;

    public async Task<ServiceResult<List<ApprovalPolicyDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var policies = await _uow.Repository<F03ApprovalPolicy>().Query()
            .AsNoTracking()
            .OrderBy(x => x.RequestType)
            .ThenBy(x => x.DeptCode)
            .ThenBy(x => x.PositionCode)
            .ThenBy(x => x.Sequence)
            .ThenBy(x => x.Level)
            .ToListAsync(ct);

        var departments = await _uow.Repository<F03Department>().Query()
            .AsNoTracking()
            .ToDictionaryAsync(x => x.DeptCode, x => x.DeptName, ct);

        var positions = await _uow.Repository<F03Position>().Query()
            .AsNoTracking()
            .ToDictionaryAsync(x => x.PositionCode, ct);

        return ServiceResult<List<ApprovalPolicyDto>>.Ok(
            policies.Select(x =>
            {
                positions.TryGetValue(x.PositionCode ?? string.Empty, out var requesterPosition);
                positions.TryGetValue(x.ApprovalPositionCode, out var approvalPosition);

                return new ApprovalPolicyDto
                {
                    Id = x.Id,
                    IsActive = x.IsActive == true,
                    RequestType = (int)x.RequestType,
                    RequestTypeName = RequestTypeName(x.RequestType),
                    DeptCode = x.DeptCode,
                    DeptName = departments.GetValueOrDefault(x.DeptCode, x.DeptCode),
                    PositionCode = x.PositionCode,
                    PositionName = requesterPosition?.PositionName ?? "(Tất cả vị trí)",
                    ApprovalPositionCode = x.ApprovalPositionCode,
                    ApprovalPositionName = approvalPosition?.PositionName ?? x.ApprovalPositionCode,
                    Level = x.Level,
                    Sequence = x.Sequence,
                    LevelName = x.LevelName,
                    RoleName = x.RoleName,
                    Required = x.Required
                };
            }).ToList());
    }

    public async Task<ServiceResult<List<ApprovalPolicyPositionDto>>> GetPositionsAsync(CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03Position>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.DefaultApproveLevel == null)
            .ThenBy(x => x.DefaultApproveLevel)
            .ThenBy(x => x.PositionCode)
            .Select(x => new ApprovalPolicyPositionDto
            {
                PositionCode = x.PositionCode,
                PositionName = x.PositionName,
                DefaultApproveLevel = x.DefaultApproveLevel,
                RoleName = RoleNameFromPosition(x.DefaultApproveLevel),
                IsActive = x.IsActive == true
            })
            .ToListAsync(ct);

        return ServiceResult<List<ApprovalPolicyPositionDto>>.Ok(rows);
    }

    public async Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03Department>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.DeptCode)
            .Select(x => new DepartmentDto
            {
                Id = x.Id,
                DeptCode = x.DeptCode,
                DeptName = x.DeptName,
                IsActive = x.IsActive == true,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return ServiceResult<List<DepartmentDto>>.Ok(rows);
    }

    public async Task<ServiceResult<ApprovalPolicyDto>> CreateAsync(
        ApprovalPolicyRequest request, int actorUserId, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(request, null, ct);
        if (validation != null) return ServiceResult<ApprovalPolicyDto>.Fail(validation);

        var entity = new F03ApprovalPolicy
        {
            RequestType = (RequestModule)request.RequestType,
            DeptCode = request.DeptCode.Trim(),
            PositionCode = Normalize(request.PositionCode),
            ApprovalPositionCode = request.ApprovalPositionCode.Trim(),
            Level = request.Level,
            Sequence = request.Sequence,
            LevelName = request.LevelName.Trim(),
            RoleName = request.RoleName.Trim(),
            Required = request.Required,
            IsActive = request.IsActive,
            CreatedBy = actorUserId,
            LastModifiedSource = "Manual"
        };

        await _uow.Repository<F03ApprovalPolicy>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<ApprovalPolicyDto>.Ok(await MapAsync(entity, ct));
    }

    public async Task<ServiceResult<ApprovalPolicyDto>> UpdateAsync(
        int id, ApprovalPolicyRequest request, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03ApprovalPolicy>().Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return ServiceResult<ApprovalPolicyDto>.Fail("Không tìm thấy Approval Policy.");

        var validation = await ValidateAsync(request, id, ct);
        if (validation != null)
            return ServiceResult<ApprovalPolicyDto>.Fail(validation);

        entity.RequestType = (RequestModule)request.RequestType;
        entity.DeptCode = request.DeptCode.Trim();
        entity.PositionCode = Normalize(request.PositionCode);
        entity.ApprovalPositionCode = request.ApprovalPositionCode.Trim();
        entity.Level = request.Level;
        entity.Sequence = request.Sequence;
        entity.LevelName = request.LevelName.Trim();
        entity.RoleName = request.RoleName.Trim();
        entity.Required = request.Required;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        entity.LastModifiedSource = "Manual";

        await _uow.SaveChangesAsync(ct);
        return ServiceResult<ApprovalPolicyDto>.Ok(await MapAsync(entity, ct));
    }

    public async Task<ServiceResult<object>> DeleteAsync(
        int id, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03ApprovalPolicy>().Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return ServiceResult<object>.Fail("Không tìm thấy Approval Policy.");

        entity.IsActive = false;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        entity.LastModifiedSource = "Manual";
        await _uow.SaveChangesAsync(ct);

        return ServiceResult<object>.Ok(new { id, deactivated = true });
    }

    private async Task<string?> ValidateAsync(
        ApprovalPolicyRequest request, int? excludeId, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(RequestModule), request.RequestType))
            return "RequestType không hợp lệ.";

        var deptCode = request.DeptCode.Trim();
        if (string.IsNullOrWhiteSpace(deptCode))
            return "Phòng ban là bắt buộc.";

        if (!await _uow.Repository<F03Department>().Query()
            .AnyAsync(x => x.IsActive == true && x.DeptCode == deptCode, ct))
            return $"Phòng ban '{deptCode}' không tồn tại hoặc đã inactive.";

        var positionCode = Normalize(request.PositionCode);
        if (positionCode != null &&
            !await _uow.Repository<F03Position>().Query()
                .AnyAsync(x => x.IsActive == true && x.PositionCode == positionCode, ct))
            return $"Position '{positionCode}' không tồn tại hoặc đã inactive.";

        var approvalPosition = await _uow.Repository<F03Position>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true &&
                        x.PositionCode == request.ApprovalPositionCode.Trim())
            .Select(x => new { x.PositionCode, x.PositionName, x.DefaultApproveLevel })
            .FirstOrDefaultAsync(ct);

        if (approvalPosition == null)
            return $"Cấp phê duyệt '{request.ApprovalPositionCode}' không tồn tại hoặc đã inactive.";

        if (!approvalPosition.DefaultApproveLevel.HasValue ||
            approvalPosition.DefaultApproveLevel.Value is < 1 or > 7)
            return $"Chức vụ '{approvalPosition.PositionName}' chưa có DefaultApproveLevel hợp lệ.";

        if (request.Level != approvalPosition.DefaultApproveLevel.Value)
            return $"Level phải bằng DefaultApproveLevel ({approvalPosition.DefaultApproveLevel}) của chức vụ phê duyệt.";

        if (request.Sequence < 1)
            return "Sequence phải >= 1.";

        if (string.IsNullOrWhiteSpace(request.LevelName))
            return "LevelName không được để trống.";

        if (string.IsNullOrWhiteSpace(request.RoleName))
            return "RoleName không được để trống.";

        var duplicate = await _uow.Repository<F03ApprovalPolicy>().Query()
            .AnyAsync(x =>
                x.IsActive == true &&
                x.Id != excludeId &&
                x.RequestType == (RequestModule)request.RequestType &&
                x.DeptCode == deptCode &&
                x.PositionCode == positionCode &&
                x.Level == request.Level,
                ct);

        return duplicate
            ? "Policy active đã tồn tại cho RequestType + Phòng ban + Position + Level."
            : null;
    }

    private async Task<ApprovalPolicyDto> MapAsync(
        F03ApprovalPolicy x, CancellationToken ct)
    {
        var deptName = await _uow.Repository<F03Department>().Query()
            .AsNoTracking()
            .Where(d => d.DeptCode == x.DeptCode)
            .Select(d => d.DeptName)
            .FirstOrDefaultAsync(ct);

        var positions = await _uow.Repository<F03Position>().Query()
            .AsNoTracking()
            .Where(p => p.PositionCode == x.PositionCode ||
                        p.PositionCode == x.ApprovalPositionCode)
            .ToDictionaryAsync(p => p.PositionCode, ct);

        positions.TryGetValue(x.PositionCode ?? string.Empty, out var requester);
        positions.TryGetValue(x.ApprovalPositionCode, out var approver);

        return new ApprovalPolicyDto
        {
            Id = x.Id,
            IsActive = x.IsActive == true,
            RequestType = (int)x.RequestType,
            RequestTypeName = RequestTypeName(x.RequestType),
            DeptCode = x.DeptCode,
            DeptName = deptName ?? x.DeptCode,
            PositionCode = x.PositionCode,
            PositionName = requester?.PositionName ?? "(Tất cả vị trí)",
            ApprovalPositionCode = x.ApprovalPositionCode,
            ApprovalPositionName = approver?.PositionName ?? x.ApprovalPositionCode,
            Level = x.Level,
            Sequence = x.Sequence,
            LevelName = x.LevelName,
            RoleName = x.RoleName,
            Required = x.Required
        };
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public async Task<bool> CanApproveAsync(
        RequestModule requestType,
        string requesterEmployeeCode,
        string approverEmployeeCode,
        int level,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(requesterEmployeeCode) ||
            string.IsNullOrWhiteSpace(approverEmployeeCode) ||
            level <= 0)
            return false;

        var requester = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == requesterEmployeeCode)
            .Select(x => new { x.DeptCode, x.PositionCode })
            .FirstOrDefaultAsync(ct);

        var approver = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == approverEmployeeCode)
            .Select(x => new { x.DeptCode, x.PositionCode })
            .FirstOrDefaultAsync(ct);

        if (requester == null || approver == null)
            return false;

        return await _uow.Repository<F03ApprovalPolicy>().Query()
            .AsNoTracking()
            .AnyAsync(x =>
                x.IsActive == true
                && x.RequestType == requestType
                && x.DeptCode == requester.DeptCode
                && (x.PositionCode == null || x.PositionCode == requester.PositionCode)
                && x.ApprovalPositionCode == approver.PositionCode
                && x.Level == level,
                ct);
    }

    private static string RoleNameFromPosition(int? level)
        => level switch
        {
            1 => ApproverRole.SubLeader,
            2 => ApproverRole.Chief,
            3 => ApproverRole.Manager,
            4 => ApproverRole.GM,
            5 => ApproverRole.Union,
            6 => ApproverRole.Manager,
            7 => ApproverRole.GM,
            _ => string.Empty
        };

    private static string RequestTypeName(RequestModule type) => type switch
    {
        RequestModule.Leave => "Nghỉ phép",
        RequestModule.Overtime => "Tăng ca",
        RequestModule.Trip => "Công tác",
        RequestModule.Equipment => "Thiết bị",
        _ => type.ToString()
    };
}
