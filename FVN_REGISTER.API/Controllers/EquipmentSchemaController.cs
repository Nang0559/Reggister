using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.EquipmentImport;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/equipment/schemas")]
[Authorize]
public sealed class EquipmentSchemaController : ControllerBase
{
    private readonly FVNWEBAPPContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public EquipmentSchemaController(FVNWEBAPPContext db, ICurrentUserService currentUser, IAuthorizationService authorization)
    {
        _db = db; _currentUser = currentUser; _authorization = authorization;
    }

    [HttpGet("departments")]
    public async Task<ActionResult<List<DepartmentDto>>> Departments(CancellationToken ct)
    {
        var user = await RequireImportAsync(ct);
        var query = _db.Departments.AsNoTracking().Where(x => x.IsActive == true);
        if (!user.IsAdmin) query = query.Where(x => x.DeptCode == user.DeptCode);
        return Ok(await query.OrderBy(x => x.DeptCode).Select(x => new DepartmentDto
        {
            Id = x.Id, DeptCode = x.DeptCode, DeptName = x.DeptName, IsActive = x.IsActive == true, CreatedAt = x.CreatedAt
        }).ToListAsync(ct));
    }

    [HttpGet]
    public async Task<ActionResult<List<EquipmentSchemaSummaryDto>>> List([FromQuery] string? deptCode, CancellationToken ct)
    {
        var user = await RequireImportAsync(ct);
        var query = _db.EquipmentSchemas.AsNoTracking().Where(x => x.IsActive == true);
        if (!string.IsNullOrWhiteSpace(deptCode))
        {
            var dept = NormalizeDept(deptCode);
            if (!await CanDeptAsync(user, dept, ct)) return Forbid();
            query = query.Where(x => x.DeptCode == dept);
        }
        else if (!user.IsAdmin) query = query.Where(x => x.DeptCode == user.DeptCode);

        return Ok(await query.OrderBy(x => x.DeptCode).ThenByDescending(x => x.Version)
            .Select(x => new EquipmentSchemaSummaryDto
            {
                Id = x.Id, DeptCode = x.DeptCode, SchemaName = x.SchemaName, Version = x.Version,
                Status = x.Status, IsActive = x.IsActive == true, FieldCount = x.Fields.Count(f => f.IsActive == true)
            }).ToListAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentSchemaDto>> Get(int id, CancellationToken ct)
    {
        var user = await RequireImportAsync(ct);
        var schema = await _db.EquipmentSchemas.AsNoTracking().Include(x => x.Fields).FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true, ct);
        if (schema == null) return NotFound();
        if (!await CanDeptAsync(user, schema.DeptCode, ct)) return Forbid();
        return Ok(Map(schema));
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentSchemaDto>> Save([FromBody] EquipmentSchemaUpsertRequest request, CancellationToken ct)
    {
        var user = await RequireImportAsync(ct);
        var dept = NormalizeDept(request.DeptCode);
        if (!await CanDeptAsync(user, dept, ct)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.SchemaName)) return BadRequest("Tên schema là bắt buộc.");
        F03EquipmentSchema? entity = request.Id.HasValue ? await _db.EquipmentSchemas.FirstOrDefaultAsync(x => x.Id == request.Id.Value && x.IsActive == true, ct) : null;
        if (request.Id.HasValue && entity == null) return NotFound();
        if (entity != null && !string.Equals(entity.DeptCode, dept, StringComparison.OrdinalIgnoreCase)) return Forbid();
        if (entity == null)
        {
            var next = (await _db.EquipmentSchemas.Where(x => x.DeptCode == dept).Select(x => (int?)x.Version).MaxAsync(ct) ?? 0) + 1;
            entity = new F03EquipmentSchema { DeptCode = dept, Version = next, CreatedBy = user.UserId };
            _db.EquipmentSchemas.Add(entity);
        }
        entity.SchemaName = request.SchemaName.Trim(); entity.Status = NormalizeStatus(request.Status); entity.ModifiedBy = user.UserId; entity.ModifiedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        if (entity.Status == "Active") await ActivateAsync(entity.Id, user.UserId, ct);
        return Ok(await GetEntityAsync(entity.Id, ct));
    }

    [HttpPost("{id:int}/clone")]
    public async Task<ActionResult<EquipmentSchemaDto>> Clone(int id, [FromBody] EquipmentSchemaCloneRequest request, CancellationToken ct)
    {
        var user = await RequireImportAsync(ct);
        var source = await _db.EquipmentSchemas.Include(x => x.Fields).FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true, ct);
        if (source == null) return NotFound();
        if (!await CanDeptAsync(user, source.DeptCode, ct)) return Forbid();
        var next = (await _db.EquipmentSchemas.Where(x => x.DeptCode == source.DeptCode).Select(x => (int?)x.Version).MaxAsync(ct) ?? 0) + 1;
        var clone = new F03EquipmentSchema { DeptCode = source.DeptCode, SchemaName = string.IsNullOrWhiteSpace(request.SchemaName) ? source.SchemaName : request.SchemaName.Trim(), Version = next, Status = "Draft", CreatedBy = user.UserId };
        _db.EquipmentSchemas.Add(clone); await _db.SaveChangesAsync(ct);
        foreach (var f in source.Fields.Where(x => x.IsActive == true))
        {
            _db.EquipmentFieldDefinitions.Add(new F03EquipmentFieldDefinition
            {
                SchemaId = clone.Id, DeptCode = clone.DeptCode, FieldKey = f.FieldKey, FieldLabel = f.FieldLabel, DataType = f.DataType,
                IsRequired = f.IsRequired, IsImportable = f.IsImportable, IsSearchable = f.IsSearchable, IsActiveField = false,
                DisplayOrder = f.DisplayOrder, MaxLength = f.MaxLength, DefaultValue = f.DefaultValue, OptionsJson = f.OptionsJson, CreatedBy = user.UserId
            });
        }
        await _db.SaveChangesAsync(ct);
        return Ok(await GetEntityAsync(clone.Id, ct));
    }

    [HttpPut("fields")]
    public async Task<ActionResult<EquipmentFieldDefinitionDto>> SaveField([FromBody] SaveEquipmentFieldDefinitionRequest request, CancellationToken ct)
    {
        var user = await RequireImportAsync(ct); var dept = NormalizeDept(request.DeptCode);
        if (!await CanDeptAsync(user, dept, ct)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.FieldKey) || string.IsNullOrWhiteSpace(request.FieldLabel)) return BadRequest("FieldKey và tên hiển thị là bắt buộc.");
        var schema = await _db.EquipmentSchemas.FirstOrDefaultAsync(x => x.Id == request.SchemaId && x.DeptCode == dept && x.IsActive == true, ct);
        if (schema == null) return BadRequest("Schema không hợp lệ.");
        var key = NormalizeKey(request.FieldKey);
        var entity = await _db.EquipmentFieldDefinitions.FirstOrDefaultAsync(x => x.SchemaId == schema.Id && x.FieldKey == key, ct);
        if (entity == null) { entity = new F03EquipmentFieldDefinition { SchemaId = schema.Id, DeptCode = dept, FieldKey = key, CreatedBy = user.UserId }; _db.EquipmentFieldDefinitions.Add(entity); }
        entity.FieldLabel = request.FieldLabel.Trim(); entity.DataType = request.DataType.Trim(); entity.IsRequired = request.IsRequired; entity.IsImportable = request.IsImportable; entity.IsSearchable = request.IsSearchable;
        entity.IsActiveField = schema.Status == "Active" && request.IsActiveField; entity.DisplayOrder = request.DisplayOrder; entity.MaxLength = request.MaxLength; entity.DefaultValue = request.DefaultValue; entity.OptionsJson = request.OptionsJson; entity.ModifiedBy = user.UserId; entity.ModifiedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct); return Ok(MapField(entity));
    }

    private async Task ActivateAsync(int schemaId, int userId, CancellationToken ct)
    {
        var target = await _db.EquipmentSchemas.FirstAsync(x => x.Id == schemaId, ct);
        var schemas = await _db.EquipmentSchemas.Where(x => x.DeptCode == target.DeptCode && x.IsActive == true).ToListAsync(ct);
        foreach (var schema in schemas) { schema.Status = schema.Id == schemaId ? "Active" : "Inactive"; schema.ModifiedBy = userId; schema.ModifiedAt = DateTime.Now; }
        var fields = await _db.EquipmentFieldDefinitions.Where(x => x.DeptCode == target.DeptCode && x.IsActive == true).ToListAsync(ct);
        foreach (var field in fields) { field.IsActiveField = field.SchemaId == schemaId; field.ModifiedBy = userId; field.ModifiedAt = DateTime.Now; }
        await _db.SaveChangesAsync(ct);
    }

    private async Task<EquipmentSchemaDto> GetEntityAsync(int id, CancellationToken ct) => Map(await _db.EquipmentSchemas.AsNoTracking().Include(x => x.Fields).FirstAsync(x => x.Id == id, ct));
    private async Task<FVN_REGISTER.Contract.Dtos.Authentication.UserIdentityDto> RequireImportAsync(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentImport, ct)) throw new UnauthorizedAccessException("Bạn chưa có quyền quản lý Equipment Schema.");
        return user;
    }
    private async Task<bool> CanDeptAsync(FVN_REGISTER.Contract.Dtos.Authentication.UserIdentityDto user, string deptCode, CancellationToken ct) => user.IsAdmin || string.Equals(user.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase) || await _authorization.CanAccessAsync(user, SecurityFunctionCodes.EquipmentImport, null, deptCode, ct);
    private static EquipmentSchemaDto Map(F03EquipmentSchema x) => new() { Id = x.Id, DeptCode = x.DeptCode, SchemaName = x.SchemaName, Version = x.Version, Status = x.Status, IsActive = x.IsActive == true, Fields = x.Fields.Where(f => f.IsActive == true).OrderBy(f => f.DisplayOrder).ThenBy(f => f.FieldLabel).Select(MapField).ToList() };
    private static EquipmentFieldDefinitionDto MapField(F03EquipmentFieldDefinition x) => new() { Id = x.Id, SchemaId = x.SchemaId, DeptCode = x.DeptCode, FieldKey = x.FieldKey, FieldLabel = x.FieldLabel, DataType = x.DataType, IsRequired = x.IsRequired, IsImportable = x.IsImportable, IsSearchable = x.IsSearchable, IsActiveField = x.IsActiveField, DisplayOrder = x.DisplayOrder, MaxLength = x.MaxLength, DefaultValue = x.DefaultValue, OptionsJson = x.OptionsJson };
    private static string NormalizeDept(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "active" => "Active", "inactive" => "Inactive", _ => "Draft" };
    private static string NormalizeKey(string value)
    {
        var formD = value.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var chars = formD.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).Select(char.ToLowerInvariant);
        return new string(chars.Where(char.IsLetterOrDigit).ToArray());
    }
}
