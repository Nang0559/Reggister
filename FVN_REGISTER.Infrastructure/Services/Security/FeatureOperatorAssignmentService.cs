
using FVN_REGISTER.Application.Interfaces.Auths;
u
using FVN_REGISTER.Application.Interfaces.FeatureOperators;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Entities.PublicForms;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.Security;

public sealed class FeatureOperatorAssignmentService : IFeatureOperatorAssignmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public FeatureOperatorAssignmentService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<List<FeatureOperatorAssignmentDto>> GetAsync(
        int functionCode, string resourceType, int? resourceId, CancellationToken ct = default)
    {
        var type = NormalizeResourceType(resourceType);
        var rows = await Query(functionCode, type, resourceId)
            .OrderBy(x => x.EmployeeCode)
            .ToListAsync(ct);
        return await EnrichAsync(rows, ct);
    }

    private async Task<List<FeatureOperatorAssignmentDto>> EnrichAsync(
        IReadOnlyCollection<F03FeatureOperatorAssignment> rows,
        CancellationToken ct)
    {
        if (rows.Count == 0) return new();

        var employeeCodes = rows.Select(x => x.EmployeeCode).Distinct().ToList();
        var employees = await _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => x.IsActive == true && employeeCodes.Contains(x.EmployeeCode))
            .Select(x => new { x.EmployeeCode, x.EmployeeName, x.DeptCode, x.PositionCode })
            .ToListAsync(ct);

        var departments = await _uow.Repository<F03Department>().Query().AsNoTracking()
            .Where(x => x.IsActive == true)
            .Select(x => new { x.DeptCode, x.DeptName })
            .ToListAsync(ct);

        var positions = await _uow.Repository<F03Position>().Query().AsNoTracking()
            .Where(x => x.IsActive == true)
            .Select(x => new { x.PositionCode, x.PositionName })
            .ToListAsync(ct);

        var functions = await _uow.Repository<F03Function>().Query().AsNoTracking()
            .Where(x => x.FunctionCode == rows.First().FunctionCode)
            .Select(x => new { x.FunctionCode, x.FunctionName })
            .FirstOrDefaultAsync(ct);

        var em = employees.ToDictionary(x => x.EmployeeCode, StringComparer.OrdinalIgnoreCase);
        var dm = departments.ToDictionary(x => x.DeptCode, x => x.DeptName, StringComparer.OrdinalIgnoreCase);
        var pm = positions.ToDictionary(x => x.PositionCode, x => x.PositionName, StringComparer.OrdinalIgnoreCase);

        return rows.Select(x =>
        {
            em.TryGetValue(x.EmployeeCode, out var e);
            dm.TryGetValue(e?.DeptCode ?? string.Empty, out var dn);
            pm.TryGetValue(e?.PositionCode ?? string.Empty, out var pn);
            return new FeatureOperatorAssignmentDto
            {
                Id = x.Id, FunctionCode = x.FunctionCode, FunctionName = functions?.FunctionName ?? string.Empty,
                ResourceType = x.ResourceType, ResourceId = x.ResourceId,
                EmployeeCode = x.EmployeeCode, EmployeeName = e?.EmployeeName ?? x.EmployeeCode,
                DeptCode = e?.DeptCode, DeptName = dn, PositionCode = e?.PositionCode, PositionName = pn,
                Remark = x.Remark
            };
        }).ToList();
    }

    public async Task<List<FeatureOperatorAssignmentDto>> GetForResourceIdsAsync(
        int functionCode, string resourceType, IReadOnlyCollection<int> resourceIds, CancellationToken ct = default)
    {
        if (resourceIds.Count == 0) return new();
        var type = NormalizeResourceType(resourceType);
        var rows = await _uow.Repository<F03FeatureOperatorAssignment>().Query()
            .Where(x => x.IsActive == true
                && x.FunctionCode == functionCode
                && x.ResourceType == type
                && x.ResourceId.HasValue
                && resourceIds.Contains(x.ResourceId.Value))
            .OrderBy(x => x.ResourceId)
            .ThenBy(x => x.EmployeeCode)
            .ToListAsync(ct);
        return await EnrichAsync(rows, ct);
    }

    public async Task<bool> CanOperateAsync(
        int userId, string? employeeCode, int functionCode, string resourceType, int? resourceId, CancellationToken ct = default)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(employeeCode)) return false;

        var type = NormalizeResourceType(resourceType);
        var baseQuery = _uow.Repository<F03FeatureOperatorAssignment>().Query().AsNoTracking()
            .Where(x => x.IsActive == true
                && x.FunctionCode == functionCode
                && x.ResourceType == type);

        if (resourceId.HasValue)
        {
            var exact = baseQuery.Where(x => x.ResourceId == resourceId.Value);
            if (await exact.AnyAsync(ct))
                return await exact.AnyAsync(x => x.EmployeeCode == employeeCode.Trim(), ct);

            var global = baseQuery.Where(x => x.ResourceId == null);
            if (await global.AnyAsync(ct))
                return await global.AnyAsync(x => x.EmployeeCode == employeeCode.Trim(), ct);

            return true; // No assignment configured: RBAC/scope remains authoritative.
        }

        if (await baseQuery.AnyAsync(ct))
            return await baseQuery.AnyAsync(x => x.EmployeeCode == employeeCode.Trim(), ct);

        return true;
    }

    public async Task<ServiceResult<FeatureOperatorAssignmentDto>> AddAsync(
        SaveFeatureOperatorAssignmentRequest request, int actorUserId, CancellationToken ct = default)
    {
        if (actorUserId <= 0) return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Người thao tác không hợp lệ.");
        if (request.FunctionCode <= 0) return ServiceResult<FeatureOperatorAssignmentDto>.Fail("FunctionCode không hợp lệ.");

        var type = NormalizeResourceType(request.ResourceType);
        if (type.Length == 0) return ServiceResult<FeatureOperatorAssignmentDto>.Fail("ResourceType là bắt buộc.");
        if (type is not ("PUBLIC_INFORMATION" or "PUBLIC_FORM" or "EXECUTION_REVIEW"))
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("ResourceType không được hỗ trợ.");
        var expectedType = request.FunctionCode switch
        {
            2801 => "PUBLIC_INFORMATION",
            2802 => "EXECUTION_REVIEW",
            2807 or 2808 or 2809 => "PUBLIC_FORM",
            _ => null
        };
        if (expectedType != null && !string.Equals(expectedType, type, StringComparison.OrdinalIgnoreCase))
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Function không phù hợp với ResourceType.");

        var employeeCode = request.EmployeeCode?.Trim() ?? string.Empty;
        var employee = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == employeeCode)
            .Select(x => new
            {
                x.EmployeeCode, x.EmployeeName, x.DeptCode, x.PositionCode,
                DeptName = _uow.Repository<F03Department>().Query()
                    .Where(d => d.DeptCode == x.DeptCode && d.IsActive == true)
                    .Select(d => d.DeptName).FirstOrDefault(),
                PositionName = _uow.Repository<F03Position>().Query()
                    .Where(p => p.PositionCode == x.PositionCode && p.IsActive == true)
                    .Select(p => p.PositionName).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (employee == null)
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Nhân viên không tồn tại hoặc đã inactive.");

        var functionExists = await _uow.Repository<F03Function>().Query()
            .AsNoTracking()
            .AnyAsync(x => x.FunctionCode == request.FunctionCode && (x.IsActive ?? true), ct);
        if (!functionExists)
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Function không tồn tại hoặc đã inactive.");

        if (request.ResourceId.HasValue && request.ResourceId.Value <= 0)
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("ResourceId không hợp lệ.");

        if (request.ResourceId.HasValue)
        {
            var resourceExists = type switch
            {
                "PUBLIC_INFORMATION" => await _uow.Repository<F03PublicInformation>().Query()
                    .AsNoTracking().AnyAsync(x => x.Id == request.ResourceId.Value && x.IsActive == true, ct),
                "PUBLIC_FORM" => await _uow.Repository<F03PublicForm>().Query()
                    .AsNoTracking().AnyAsync(x => x.Id == request.ResourceId.Value && x.IsActive == true, ct),
                "EXECUTION_REVIEW" => false,
                _ => false
            };

            if (!resourceExists)
                return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Resource không tồn tại hoặc đã inactive.");
        }

        if (type == "EXECUTION_REVIEW" && request.ResourceId.HasValue)
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Execution Review là assignment cấp module; ResourceId phải để trống.");

        var exists = await _uow.Repository<F03FeatureOperatorAssignment>().Query()
            .AnyAsync(x => x.IsActive == true
                && x.FunctionCode == request.FunctionCode
                && x.ResourceType == type
                && x.ResourceId == request.ResourceId
                && x.EmployeeCode == employeeCode, ct);
        if (exists)
            return ServiceResult<FeatureOperatorAssignmentDto>.Fail("Nhân viên đã được chỉ định.");

        var entity = new F03FeatureOperatorAssignment
        {
            FunctionCode = request.FunctionCode,
            ResourceType = type,
            ResourceId = request.ResourceId,
            EmployeeCode = employeeCode,
            Remark = request.Remark?.Trim(),
            CreatedBy = actorUserId
        };

        await _uow.Repository<F03FeatureOperatorAssignment>().AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAction("SECURITY_FEATURE_OPERATOR_ADDED", actorUserId,
            $"Id={entity.Id}; Function={entity.FunctionCode}; Resource={entity.ResourceType}:{entity.ResourceId}; Employee={entity.EmployeeCode}", ct: ct);

        return ServiceResult<FeatureOperatorAssignmentDto>.Ok(new FeatureOperatorAssignmentDto
        {
            Id = entity.Id,
            FunctionCode = entity.FunctionCode,
            ResourceType = entity.ResourceType,
            ResourceId = entity.ResourceId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.EmployeeName ?? employee.EmployeeCode,
            DeptCode = employee.DeptCode,
            DeptName = employee.DeptName,
            PositionCode = employee.PositionCode,
            PositionName = employee.PositionName,
            Remark = entity.Remark
        });
    }

    public async Task<ServiceResult> RemoveAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03FeatureOperatorAssignment>().Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true, ct);
        if (entity == null) return ServiceResult.Fail("Không tìm thấy chỉ định.");

        entity.IsActive = false;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        entity.LastModifiedSource = "SECURITY_FEATURE_OPERATOR_REMOVE";
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAction("SECURITY_FEATURE_OPERATOR_REMOVED", actorUserId,
            $"Id={id}; Function={entity.FunctionCode}; Resource={entity.ResourceType}:{entity.ResourceId}; Employee={entity.EmployeeCode}", ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<List<FeatureOperatorEmployeeDto>> GetEmployeesAsync(string? search, CancellationToken ct = default)
    {
        var term = search?.Trim();
        var q = _uow.Repository<F03Employee>().Query().AsNoTracking()
            .Where(x => x.IsActive == true
                && (!x.EndWorkingDate.HasValue || x.EndWorkingDate.Value.Date >= DateTime.Today));

        if (!string.IsNullOrWhiteSpace(term))
            q = q.Where(x => x.EmployeeCode.Contains(term) || x.EmployeeName.Contains(term)
                || x.DeptCode.Contains(term) || x.PositionCode.Contains(term));

        return await q.OrderBy(x => x.EmployeeCode).Take(500)
            .Select(x => new FeatureOperatorEmployeeDto
            {
                EmployeeCode = x.EmployeeCode,
                EmployeeName = x.EmployeeName ?? x.EmployeeCode,
                DeptCode = x.DeptCode,
                DeptName = _uow.Repository<F03Department>().Query()
                    .Where(d => d.DeptCode == x.DeptCode && d.IsActive == true)
                    .Select(d => d.DeptName).FirstOrDefault(),
                PositionCode = x.PositionCode,
                PositionName = _uow.Repository<F03Position>().Query()
                    .Where(p => p.PositionCode == x.PositionCode && p.IsActive == true)
                    .Select(p => p.PositionName).FirstOrDefault()
            })
            .ToListAsync(ct);
    }

    public async Task<List<FeatureOperatorResourceDto>> GetResourcesAsync(string resourceType, CancellationToken ct = default)
    {
        var type = NormalizeResourceType(resourceType);
        return type switch
        {
            "PUBLIC_INFORMATION" => await _uow.Repository<F03PublicInformation>().Query()
                .AsNoTracking().Where(x => x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new FeatureOperatorResourceDto { Id = x.Id, Code = x.Id.ToString(), Name = x.Title, Status = x.Status })
                .ToListAsync(ct),
            "PUBLIC_FORM" => await _uow.Repository<F03PublicForm>().Query()
                .AsNoTracking().Where(x => x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new FeatureOperatorResourceDto { Id = x.Id, Code = x.FormCode, Name = x.Title, Status = x.Status })
                .ToListAsync(ct),
            "EXECUTION_REVIEW" => new List<FeatureOperatorResourceDto>(),
            _ => throw new ArgumentException($"ResourceType không được hỗ trợ: {resourceType}")
        };
    }

    private IQueryable<F03FeatureOperatorAssignment> Query(int functionCode, string type, int? resourceId)
    {
        var q = _uow.Repository<F03FeatureOperatorAssignment>().Query().AsNoTracking()
            .Where(x => x.IsActive == true && x.FunctionCode == functionCode && x.ResourceType == type);
        return resourceId.HasValue ? q.Where(x => x.ResourceId == resourceId) : q.Where(x => x.ResourceId == null);
    }

    private static string NormalizeResourceType(string value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}