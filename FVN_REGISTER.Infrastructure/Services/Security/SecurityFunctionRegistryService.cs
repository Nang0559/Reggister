using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Attributes;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

/// <summary>Phát hiện chức năng và đối chiếu với danh mục bảo mật mà không tự cấp hoặc thu hồi quyền.</summary>
public sealed class SecurityFunctionRegistryService
{
    private readonly FVNWEBAPPContext _db;
    public SecurityFunctionRegistryService(FVNWEBAPPContext db) => _db = db;

    public async Task<bool> CanBootstrapRegistryAsync(int userId, CancellationToken ct = default)
    {
        if (userId <= 0) return false;
        var isSuperAdmin = await _db.Users.AsNoTracking().AnyAsync(
            x => x.Id == userId && x.IsActive == true && x.PermissionCode == 1, ct);
        if (isSuperAdmin) return true;

        return await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.IdRole equals r.Id
            where ur.IdUser == userId && r.IsActive == true && r.RoleCode == 1
            select ur.Id).AnyAsync(ct);
    }

    public async Task<SecurityFunctionDiscoverySummaryDto> ReconcileAsync(CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var discovered = DiscoverDefinitions();
        var functions = await _db.Functions.ToListAsync(ct);
        var registry = await _db.SecurityFunctionRegistry.ToListAsync(ct);
        var registryByKey = registry.ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);
        var functionByKey = functions.Where(x => !string.IsNullOrWhiteSpace(x.FunctionKey)).ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);
        var functionByCode = functions.GroupBy(x => x.FunctionCode).ToDictionary(x => x.Key, x => x.ToList());
        var matched = 0; var pending = 0; var retirement = 0; var conflicts = 0;

        foreach (var d in discovered.Values)
        {
            var codeConflict = d.FunctionCode > 0 && functionByCode.TryGetValue(d.FunctionCode, out var sameCode) && sameCode.Any(x => !string.Equals(x.FunctionKey, d.FunctionKey, StringComparison.OrdinalIgnoreCase));
            if (!registryByKey.TryGetValue(d.FunctionKey, out var item))
            {
                item = new F03SecurityFunctionRegistryItem
                {
                    FunctionKey = d.FunctionKey, FunctionCode = d.FunctionCode, DefinitionName = d.DefinitionName,
                    ModuleCode = d.ModuleCode, ActionCode = d.ActionCode, ScopeCode = d.ScopeCode,
                    LifecycleStatus = codeConflict ? "Conflict" : functionByKey.ContainsKey(d.FunctionKey) ? "Active" : "PendingRegistration",
                    SourceType = d.SourceType, SourceAssembly = d.SourceAssembly, SourceTypeName = d.SourceTypeName,
                    DefinitionHash = d.DefinitionHash, FirstDiscoveredAt = now, LastSeenAt = now
                };
                _db.SecurityFunctionRegistry.Add(item); registryByKey[d.FunctionKey] = item;
            }
            else
            {
                item.LastSeenAt = now; item.DefinitionHash = d.DefinitionHash; item.DefinitionName = d.DefinitionName; item.FunctionCode = d.FunctionCode;
                item.ModuleCode = d.ModuleCode; item.ActionCode = d.ActionCode; item.ScopeCode = d.ScopeCode; item.SourceType = d.SourceType;
                item.SourceAssembly = d.SourceAssembly; item.SourceTypeName = d.SourceTypeName;
                if (codeConflict) { item.LifecycleStatus = "Conflict"; item.ResolvedAt = null; }
                else if (functionByKey.TryGetValue(d.FunctionKey, out var f))
                {
                    if (f.LifecycleStatus == "PendingRetirement") { f.LifecycleStatus = "Active"; f.IsActive = true; }
                    item.LifecycleStatus = f.LifecycleStatus; item.ResolvedAt ??= now; matched++;
                }
                else if (!item.IsIgnored && item.LifecycleStatus is not ("Retired" or "Replaced")) { item.LifecycleStatus = "PendingRegistration"; item.ResolvedAt = null; }
            }
            switch (item.LifecycleStatus) { case "PendingRegistration": pending++; break; case "Conflict": conflicts++; break; }
        }

        var discoveredKeys = discovered.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var f in functions)
        {
            if (string.IsNullOrWhiteSpace(f.FunctionKey) || discoveredKeys.Contains(f.FunctionKey) || f.LifecycleStatus is "Retired" or "Replaced") continue;
            f.LifecycleStatus = "PendingRetirement"; f.IsActive = false; retirement++;
            if (registryByKey.TryGetValue(f.FunctionKey, out var item)) { item.LifecycleStatus = "PendingRetirement"; item.ResolvedAt = null; }
            else _db.SecurityFunctionRegistry.Add(new F03SecurityFunctionRegistryItem
            {
                FunctionKey = f.FunctionKey, FunctionCode = f.FunctionCode, DefinitionName = f.FunctionName, ModuleCode = f.ModuleCode,
                ActionCode = f.ActionCode, ScopeCode = f.ScopeCode, LifecycleStatus = "PendingRetirement", SourceType = f.SourceType,
                DefinitionHash = Hash(f.FunctionKey + "|missing"), FirstDiscoveredAt = f.CreatedAt, LastSeenAt = f.LastSeenAt ?? f.CreatedAt
            });
        }
        await _db.SaveChangesAsync(ct);
        return new SecurityFunctionDiscoverySummaryDto(now, discovered.Count, matched, pending, retirement, conflicts);
    }

    public async Task<IReadOnlyList<SecurityFunctionRegistryItemDto>> GetRegistryAsync(string? status = null, CancellationToken ct = default)
    {
        var query = _db.SecurityFunctionRegistry.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.LifecycleStatus == status);
        var items = await query.OrderBy(x => x.LifecycleStatus).ThenBy(x => x.FunctionKey).ToListAsync(ct);
        var registered = (await _db.Functions.AsNoTracking().Select(x => x.FunctionKey).ToListAsync(ct)).Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return items.Select(x => new SecurityFunctionRegistryItemDto(x.Id, x.FunctionKey, x.FunctionCode, x.DefinitionName, x.ModuleCode, x.ActionCode, x.ScopeCode, x.LifecycleStatus, x.SourceType, x.SourceAssembly, x.SourceTypeName, x.ReplacementFunctionKey, x.FirstDiscoveredAt, x.LastSeenAt, x.ResolvedAt, x.IsIgnored, registered.Contains(x.FunctionKey))).ToList();
    }

    public async Task<SecurityFunctionRegistryItemDto?> GetAsync(string functionKey, CancellationToken ct = default) => (await GetRegistryAsync(null, ct)).FirstOrDefault(x => string.Equals(x.FunctionKey, functionKey, StringComparison.OrdinalIgnoreCase));

    public async Task RegisterAsync(string functionKey, RegisterDiscoveredFunctionRequest request, int actorUserId, CancellationToken ct = default)
    {
        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct) ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}' trong danh mục chức năng.");
        if (item.LifecycleStatus == "Conflict") throw new InvalidOperationException($"Chức năng '{functionKey}' đang có xung đột mã chức năng và phải được xử lý trước.");
        if (await _db.Functions.AnyAsync(x => x.FunctionKey == functionKey, ct)) throw new InvalidOperationException($"Chức năng '{functionKey}' đã được đăng ký.");
        if (item.FunctionCode <= 0) throw new InvalidOperationException($"Chức năng '{functionKey}' chưa có mã chức năng hợp lệ.");
        if (await _db.Functions.AnyAsync(x => x.FunctionCode == item.FunctionCode, ct)) throw new InvalidOperationException($"Mã chức năng {item.FunctionCode} đã tồn tại trong danh mục.");
        var functionName = string.IsNullOrWhiteSpace(request.FunctionName) ? item.DefinitionName : request.FunctionName.Trim();
        var detail = string.IsNullOrWhiteSpace(request.Detail) ? SecurityFunctionCatalog.GetDescription(item.FunctionKey) : request.Detail.Trim();
        var moduleCode = string.IsNullOrWhiteSpace(request.ModuleCode) ? item.ModuleCode : request.ModuleCode.Trim();
        var actionCode = string.IsNullOrWhiteSpace(request.ActionCode) ? item.ActionCode : request.ActionCode.Trim();
        var scopeCode = string.IsNullOrWhiteSpace(request.ScopeCode) || string.Equals(request.ScopeCode, "None", StringComparison.OrdinalIgnoreCase) ? item.ScopeCode : request.ScopeCode.Trim();
        var f = new F03Function
        {
            FunctionCode = item.FunctionCode, FunctionKey = item.FunctionKey, FunctionName = functionName,
            Detail = detail, ModuleCode = moduleCode, ActionCode = actionCode, ScopeCode = NormalizeScope(scopeCode), LifecycleStatus = "Active",
            SourceType = item.SourceType, LastSeenAt = item.LastSeenAt, DisplayOrder = item.FunctionCode, CreatedBy = actorUserId, IsActive = true
        };
        _db.Functions.Add(f); item.LifecycleStatus = "Active"; item.IsIgnored = false; item.ResolvedAt = DateTime.Now; await _db.SaveChangesAsync(ct);
    }

    public async Task RetireAsync(string functionKey, int actorUserId, CancellationToken ct = default)
    {
        var f = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct) ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}'.");
        f.LifecycleStatus = "Retired"; f.IsActive = false; f.ModifiedBy = actorUserId; f.ModifiedAt = DateTime.Now;
        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct); if (item != null) { item.LifecycleStatus = "Retired"; item.ResolvedAt = DateTime.Now; }
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceAsync(string functionKey, ReplaceFunctionRequest request, int actorUserId, CancellationToken ct = default)
    {
        if (string.Equals(functionKey, request.ReplacementFunctionKey, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Không thể thay thế chức năng bằng chính nó.");
        var old = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct) ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}'.");
        var replacement = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == request.ReplacementFunctionKey, ct) ?? throw new InvalidOperationException($"Không tìm thấy chức năng thay thế '{request.ReplacementFunctionKey}'.");
        if (!replacement.IsActive || replacement.LifecycleStatus != "Active") throw new InvalidOperationException("Chức năng thay thế phải đang hoạt động.");
        old.LifecycleStatus = "Replaced"; old.ReplacementFunctionKey = replacement.FunctionKey; old.IsActive = false; old.ModifiedBy = actorUserId; old.ModifiedAt = DateTime.Now;
        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct); if (item != null) { item.LifecycleStatus = "Replaced"; item.ReplacementFunctionKey = replacement.FunctionKey; item.ResolvedAt = DateTime.Now; }
        await _db.SaveChangesAsync(ct);
    }

    public async Task IgnoreAsync(string functionKey, int actorUserId, CancellationToken ct = default)
    {
        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct) ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}'.");
        item.IsIgnored = true; item.LifecycleStatus = "Ignored"; item.ResolvedAt = DateTime.Now; item.ModifiedBy = actorUserId; item.ModifiedAt = DateTime.Now; await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertFunctionAsync(SecurityFunctionUpsertRequest request, int actorUserId, CancellationToken ct = default)
    {
        var key = request.FunctionKey?.Trim(); if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Mã định danh chức năng không được để trống.");
        if (request.FunctionCode <= 0) throw new InvalidOperationException("Mã chức năng phải lớn hơn 0.");
        var byKey = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == key, ct);
        var byCode = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionCode == request.FunctionCode, ct);
        if (byKey != null && byCode != null && byKey.Id != byCode.Id) throw new InvalidOperationException("Mã định danh và mã chức năng đang thuộc hai chức năng khác nhau.");
        var entity = byKey ?? byCode;
        if (entity == null) { entity = new F03Function { FunctionCode = request.FunctionCode, CreatedBy = actorUserId }; _db.Functions.Add(entity); }
        entity.FunctionCode = request.FunctionCode; entity.FunctionKey = key;
        entity.FunctionName = string.IsNullOrWhiteSpace(request.FunctionName) ? SecurityFunctionCatalog.GetDisplayName(key) : request.FunctionName.Trim();
        entity.Detail = string.IsNullOrWhiteSpace(request.Detail) ? SecurityFunctionCatalog.GetDescription(key) : request.Detail.Trim();
        entity.ModuleCode = request.ModuleCode?.Trim(); entity.ActionCode = request.ActionCode?.Trim(); entity.ScopeCode = NormalizeScope(request.ScopeCode); entity.DisplayOrder = request.DisplayOrder == 0 ? request.FunctionCode : request.DisplayOrder;
        entity.LifecycleStatus = "Active"; entity.SourceType = "Manual"; entity.IsActive = true; entity.ModifiedBy = actorUserId; entity.ModifiedAt = DateTime.Now; await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteFunctionAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var f = await _db.Functions.Include(x => x.RoleFunctions).Include(x => x.UserFunctions).SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new InvalidOperationException("Không tìm thấy chức năng.");
        if (f.RoleFunctions.Count > 0 || f.UserFunctions.Count > 0 || f.SourceType != "Manual") { f.LifecycleStatus = "Retired"; f.IsActive = false; f.ModifiedBy = actorUserId; f.ModifiedAt = DateTime.Now; } else _db.Functions.Remove(f);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertRoleAsync(SecurityRoleUpsertRequest request, int actorUserId, CancellationToken ct = default)
    {
        if (request.RoleCode <= 0) throw new InvalidOperationException("Mã vai trò phải lớn hơn 0.");
        if (string.IsNullOrWhiteSpace(request.RoleName)) throw new InvalidOperationException("Tên vai trò không được để trống.");
        var role = await _db.Roles.SingleOrDefaultAsync(x => x.RoleCode == request.RoleCode, ct);
        if (role == null) { role = new F03Role { RoleCode = request.RoleCode, CreatedBy = actorUserId }; _db.Roles.Add(role); }
        if (role.IsSystem && !request.IsSystem) throw new InvalidOperationException("Không được hạ vai trò hệ thống thành vai trò thường.");
        role.RoleName = request.RoleName.Trim(); role.Detail = request.Detail?.Trim(); role.IsSystem = role.IsSystem || request.IsSystem; role.IsActive = true; role.ModifiedBy = actorUserId; role.ModifiedAt = DateTime.Now; await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteRoleAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var role = await _db.Roles.Include(x => x.RoleFunctions).Include(x => x.UserRoles).SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new InvalidOperationException("Không tìm thấy vai trò.");
        if (role.IsSystem || role.RoleFunctions.Count > 0 || role.UserRoles.Count > 0) { role.IsActive = false; role.ModifiedBy = actorUserId; role.ModifiedAt = DateTime.Now; } else _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);
    }

    private static Dictionary<string, DiscoveredDefinition> DiscoverDefinitions()
    {
        var result = new Dictionary<string, DiscoveredDefinition>(StringComparer.OrdinalIgnoreCase);
        var constants = typeof(SecurityFunctionCodes);
        foreach (var field in constants.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(int)) continue; var code = (int)(field.GetValue(null) ?? 0); if (code <= 0) continue;
            var attr = field.GetCustomAttribute<SecurityFunctionDefinitionAttribute>(); var key = attr?.FunctionKey ?? ToFunctionKey(field.Name);
            var name = attr?.DisplayName ?? SecurityFunctionCatalog.GetDisplayName(key);
            var module = attr?.ModuleCode ?? GetModule(key); var action = attr?.ActionCode ?? GetAction(key); var scope = attr?.ScopeCode;
            result[key] = new DiscoveredDefinition(key, code, name, module, action, scope, "SecurityFunctionCodes", constants.Assembly.GetName().Name, constants.FullName, Hash($"{key}|{code}|{name}|{module}|{action}|{scope}"));
        }
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
        {
            Type[] types; try { types = assembly.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(x => x != null).Cast<Type>().ToArray(); }
            foreach (var type in types) foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attr = member.GetCustomAttribute<SecurityFunctionDefinitionAttribute>(); if (attr == null || string.IsNullOrWhiteSpace(attr.FunctionKey)) continue;
                var code = member is FieldInfo fi && fi.FieldType == typeof(int) ? (int)(fi.GetValue(null) ?? 0) : result.GetValueOrDefault(attr.FunctionKey)?.FunctionCode ?? 0;
                var name = attr.DisplayName ?? SecurityFunctionCatalog.GetDisplayName(attr.FunctionKey); var module = attr.ModuleCode ?? GetModule(attr.FunctionKey); var action = attr.ActionCode ?? GetAction(attr.FunctionKey);
                result[attr.FunctionKey] = new DiscoveredDefinition(attr.FunctionKey, code, name, module, action, attr.ScopeCode, "Attribute", assembly.GetName().Name, type.FullName, Hash($"{attr.FunctionKey}|{code}|{name}|{module}|{action}|{attr.ScopeCode}"));
            }
        }
        return result;
    }

    private static string ToFunctionKey(string name)
    {
        var modules = new[] { "SecurityAccessChange", "UserManagement", "PublicInformation", "PublicForm", "WorkCalendar", "DepartmentStatus", "ApprovalPolicy", "HrmUserRoleRule", "EmailQueue", "EmailTemplate", "Equipment", "HrmSync", "Attendance", "Dashboard", "LeaveType", "Department", "Employee", "Approver", "OTLimit", "Execution", "Payroll", "Security", "Leave", "Trip", "OT" };
        var module = modules.OrderByDescending(x => x.Length).FirstOrDefault(name.StartsWith); if (module == null) return name;
        var suffix = name[module.Length..]; return string.IsNullOrWhiteSpace(suffix) ? module : $"{module}.{suffix}";
    }
    private static string GetModule(string key) => key.Split('.', 2)[0];
    private static string? GetAction(string key) => key.Contains('.') ? key[(key.IndexOf('.') + 1)..] : null;
    private static string NormalizeScope(string? scope) => string.IsNullOrWhiteSpace(scope) ? "None" : scope.Trim();
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private sealed record DiscoveredDefinition(string FunctionKey, int FunctionCode, string DefinitionName, string? ModuleCode, string? ActionCode, string? ScopeCode, string SourceType, string? SourceAssembly, string? SourceTypeName, string DefinitionHash);
}
