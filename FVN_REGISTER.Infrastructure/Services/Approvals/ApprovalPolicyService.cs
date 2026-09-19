using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Enums;
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
            .OrderBy(x => x.RequestType).ThenBy(x => x.Sequence).ThenBy(x => x.Level).ThenBy(x => x.PositionCode)
            .ToListAsync(ct);

        var positions = await _uow.Repository<F03Position>().Query().AsNoTracking().ToListAsync(ct);
        var map = positions.ToDictionary(x => x.PositionCode, x => x.PositionName);

        return ServiceResult<List<ApprovalPolicyDto>>.Ok(
            policies.Select(x => new ApprovalPolicyDto
            {
                Id = x.Id,
                IsActive = x.IsActive == true,
                RequestType = (int)x.RequestType,
                RequestTypeName = RequestTypeName(x.RequestType),
                PositionCode = x.PositionCode,
                PositionName = map.GetValueOrDefault(x.PositionCode, x.PositionCode),
                Level = x.Level,
                Sequence = x.Sequence,
                LevelName = x.LevelName,
                RoleName = x.RoleName,
                Required = x.Required
            }).ToList());
    }

    public async Task<ServiceResult<List<ApprovalPolicyPositionDto>>> GetPositionsAsync(CancellationToken ct = default)
    {
        var rows = await _uow.Repository<F03Position>().Query().AsNoTracking()
            .Where(x => x.IsActive == true)
            .OrderBy(x => x.PositionCode)
            .Select(x => new ApprovalPolicyPositionDto
            {
                PositionCode = x.PositionCode,
                PositionName = x.PositionName
            })
            .ToListAsync(ct);

        return ServiceResult<List<ApprovalPolicyPositionDto>>.Ok(rows);
    }

    public async Task<ServiceResult<ApprovalPolicyDto>> CreateAsync(
        ApprovalPolicyRequest request, int actorUserId, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(request, null, ct);
        if (validation != null) return ServiceResult<ApprovalPolicyDto>.Fail(validation);

        var entity = new F03ApprovalPolicy
        {
            RequestType = (RequestModule)request.RequestType,
            PositionCode = request.PositionCode.Trim(),
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

        if (entity == null) return ServiceResult<ApprovalPolicyDto>.Fail("Không tìm thấy Approval Policy.");

        var validation = await ValidateAsync(request, id, ct);
        if (validation != null) return ServiceResult<ApprovalPolicyDto>.Fail(validation);

        entity.RequestType = (RequestModule)request.RequestType;
        entity.PositionCode = request.PositionCode.Trim();
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

    public async Task<ServiceResult<object>> DeleteAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03ApprovalPolicy>().Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null) return ServiceResult<object>.Fail("Không tìm thấy Approval Policy.");

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

        if (string.IsNullOrWhiteSpace(request.PositionCode))
            return "Chưa chọn PositionCode.";

        if (!await _uow.Repository<F03Position>().Query()
            .AnyAsync(x => x.IsActive == true && x.PositionCode == request.PositionCode.Trim(), ct))
            return $"PositionCode '{request.PositionCode}' không tồn tại trong F03Positions.";

        if (request.Level is < 1 or > 7)
            return "Level phải từ 1 đến 7.";

        if (request.Sequence < 1)
            return "Sequence phải >= 1.";

        if (string.IsNullOrWhiteSpace(request.LevelName))
            return "LevelName không được để trống.";

        if (string.IsNullOrWhiteSpace(request.RoleName))
            return "RoleName không được để trống.";

        var duplicate = await _uow.Repository<F03ApprovalPolicy>().Query()
            .AnyAsync(x => x.IsActive == true &&
                           x.Id != excludeId &&
                           x.RequestType == (RequestModule)request.RequestType &&
                           x.PositionCode == request.PositionCode.Trim(), ct);

        return duplicate
            ? "Policy đang active đã tồn tại cho RequestType + PositionCode này."
            : null;
    }

    private async Task<ApprovalPolicyDto> MapAsync(F03ApprovalPolicy x, CancellationToken ct)
    {
        var positionName = await _uow.Repository<F03Position>().Query()
            .AsNoTracking()
            .Where(p => p.PositionCode == x.PositionCode)
            .Select(p => p.PositionName)
            .FirstOrDefaultAsync(ct);

        return new ApprovalPolicyDto
        {
            Id = x.Id,
            IsActive = x.IsActive == true,
            RequestType = (int)x.RequestType,
            RequestTypeName = RequestTypeName(x.RequestType),
            PositionCode = x.PositionCode,
            PositionName = positionName ?? x.PositionCode,
            Level = x.Level,
            Sequence = x.Sequence,
            LevelName = x.LevelName,
            RoleName = x.RoleName,
            Required = x.Required
        };
    }

    private static string RequestTypeName(RequestModule type) => type switch
    {
        RequestModule.Leave => "Nghỉ phép",
        RequestModule.Overtime => "Tăng ca",
        RequestModule.Trip => "Công tác",
        RequestModule.Equipment => "Thiết bị",
        _ => type.ToString()
    };
}
