using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Attributes;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

/// <summary>
/// Discovers security capabilities declared by the application and reconciles them
/// with the persistent permission catalog. Discovery never grants, revokes or deletes access.
/// </summary>
public sealed class SecurityFunctionRegistryService
{
    private readonly FVNWEBAPPContext _db;

    public SecurityFunctionRegistryService(FVNWEBAPPContext db) => _db = db;

    public async Task<SecurityFunctionDiscoverySummaryDto> ReconcileAsync(CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var discovered = DiscoverDefinitions();
        var dbFunctions = await _db.Functions.ToListAsync(ct);
        var registry = await _db.SecurityFunctionRegistry.ToListAsync(ct);
        var registryByKey = registry.ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);
        var dbByKey = dbFunctions.Where(x => !string.IsNullOrWhiteSpace(x.FunctionKey))
            .ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);

        var matched = 0;
        var pendingRegistration = 0;
        var pendingRetirement = 0;
        var conflict = 0;

        foreach (var definition in discovered.Values)
        {
            if (registryByKey.TryGetValue(definition.FunctionKey, out var item))
            {
                item.LastSeenAt = now;
                item.DefinitionHash = definition.DefinitionHash;
                item.DefinitionName = definition.DefinitionName;
                item.FunctionCode = definition.FunctionCode;
                item.ModuleCode = definition.ModuleCode;
                item.ActionCode = definition.ActionCode;
                item.ScopeCode = definition.ScopeCode;
                item.SourceAssembly = definition.SourceAssembly;
                item.SourceTypeName = definition.SourceTypeName;
                item.IsIgnored = item.IsIgnored && !dbByKey.ContainsKey(definition.FunctionKey);

                if (dbByKey.TryGetValue(definition.FunctionKey, out var function))
                {
                    function.LastSeenAt = now;
                    if (function.LifecycleStatus == "PendingRetirement")
                        function.LifecycleStatus = "Active";
                    if (function.LifecycleStatus == "Active")
                        function.IsActive = true;
                    item.LifecycleStatus = function.LifecycleStatus ?? "Active";
                    item.ResolvedAt ??= now;
                    matched++;
                }
                else if (item.IsIgnored)
                {
                    item.LifecycleStatus = "Ignored";
                }
                else if (item.LifecycleStatus is "Retired" or "Replaced")
                {
                    item.LifecycleStatus = "Conflict";
                    item.ResolvedAt = null;
                    conflict++;
                }
                else
                {
                    item.LifecycleStatus = "PendingRegistration";
                    item.ResolvedAt = null;
                    pendingRegistration++;
                }
            }
            else
            {
                item = new F03SecurityFunctionRegistryItem
                {
                    FunctionKey = definition.FunctionKey,
                    FunctionCode = definition.FunctionCode,
                    DefinitionName = definition.DefinitionName,
                    ModuleCode = definition.ModuleCode,
                    ActionCode = definition.ActionCode,
                    ScopeCode = definition.ScopeCode,
                    LifecycleStatus = dbByKey.ContainsKey(definition.FunctionKey) ? "Active" : "PendingRegistration",
                    SourceType = definition.SourceType,
                    SourceAssembly = definition.SourceAssembly,
                    SourceTypeName = definition.SourceTypeName,
                    DefinitionHash = definition.DefinitionHash,
                    FirstDiscoveredAt = now,
                    LastSeenAt = now,
                    IsIgnored = false
                };
                _db.SecurityFunctionRegistry.Add(item);
                registryByKey[item.FunctionKey] = item;
                if (dbByKey.ContainsKey(definition.FunctionKey)) matched++; else pendingRegistration++;
            }
        }

        var discoveredKeys = discovered.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var function in dbFunctions)
        {
            if (string.IsNullOrWhiteSpace(function.FunctionKey))
                continue;

            if (discoveredKeys.Contains(function.FunctionKey))
                continue;

            if (function.LifecycleStatus is "Retired" or "Replaced")
                continue;

            function.LifecycleStatus = "PendingRetirement";
            function.IsActive = false;
            pendingRetirement++;

            if (!registryByKey.TryGetValue(function.FunctionKey, out var item))
            {
                item = new F03SecurityFunctionRegistryItem
                {
                    FunctionKey = function.FunctionKey,
                    FunctionCode = function.FunctionCode,
                    DefinitionName = function.FunctionName,
                    ModuleCode = function.ModuleCode,
                    ActionCode = function.ActionCode,
                    ScopeCode = function.ScopeCode,
                    LifecycleStatus = "PendingRetirement",
                    SourceType = function.SourceType,
                    DefinitionHash = Hash(function.FunctionKey + "|missing"),
                    FirstDiscoveredAt = function.CreatedAt,
                    LastSeenAt = function.LastSeenAt ?? function.CreatedAt,
                    IsIgnored = false
                };
                _db.SecurityFunctionRegistry.Add(item);
            }
            else
            {
                item.LifecycleStatus = "PendingRetirement";
                item.ResolvedAt = null;
            }
        }

        await _db.SaveChangesAsync(ct);
        return new SecurityFunctionDiscoverySummaryDto(now, discovered.Count, matched, pendingRegistration, pendingRetirement, conflict);
    }

    public async Task<IReadOnlyList<SecurityFunctionRegistryItemDto>> GetRegistryAsync(string? status = null, CancellationToken ct = default)
    {
        var query = _db.SecurityFunctionRegistry.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.LifecycleStatus == status);

        var items = await query.OrderBy(x => x.LifecycleStatus).ThenBy(x => x.FunctionKey).ToListAsync(ct);
        var keys = await _db.Functions.AsNoTracking().Select(x => x.FunctionKey).ToListAsync(ct);
        var registered = keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return items.Select(x => new SecurityFunctionRegistryItemDto(
            x.Id, x.FunctionKey, x.FunctionCode, x.DefinitionName, x.ModuleCode, x.ActionCode,
            x.ScopeCode, x.LifecycleStatus, x.SourceType, x.SourceAssembly, x.SourceTypeName,
            x.ReplacementFunctionKey, x.FirstDiscoveredAt, x.LastSeenAt, x.ResolvedAt,
            x.IsIgnored, registered.Contains(x.FunctionKey))).ToList();
    }

    public async Task<SecurityFunctionRegistryItemDto?> GetAsync(string functionKey, CancellationToken ct = default)
        => (await GetRegistryAsync(null, ct)).FirstOrDefault(x => string.Equals(x.FunctionKey, functionKey, StringComparison.OrdinalIgnoreCase));

    public async Task RegisterAsync(string functionKey, RegisterDiscoveredFunctionRequest request, int actorUserId, CancellationToken ct = default)
    {
        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}' trong Function Registry.");

        if (await _db.Functions.AnyAsync(x => x.FunctionCode == item.FunctionCode, ct))
            throw new InvalidOperationException($"FunctionCode {item.FunctionCode} đã tồn tại trong F03Functions.");
        if (await _db.Functions.AnyAsync(x => x.FunctionKey == functionKey, ct))
            throw new InvalidOperationException($"FunctionKey '{functionKey}' đã được đăng ký.");

        var function = new F03Function
        {
            FunctionCode = item.FunctionCode,
            FunctionKey = item.FunctionKey,
            FunctionName = request.FunctionName,
            Detail = request.Detail,
            ModuleCode = request.ModuleCode,
            ActionCode = request.ActionCode,
            ScopeCode = request.ScopeCode,
            LifecycleStatus = "Active",
            SourceType = item.SourceType,
            LastSeenAt = item.LastSeenAt,
            DisplayOrder = item.FunctionCode,
            CreatedBy = actorUserId,
            IsActive = true
        };
        _db.Functions.Add(function);
        item.LifecycleStatus = "Active";
        item.IsIgnored = false;
        item.ResolvedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RetireAsync(string functionKey, int actorUserId, CancellationToken ct = default)
    {
        var function = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}'.");

        function.LifecycleStatus = "Retired";
        function.IsActive = false;
        function.ModifiedBy = actorUserId;
        function.ModifiedAt = DateTime.Now;

        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct);
        if (item != null)
        {
            item.LifecycleStatus = "Retired";
            item.ResolvedAt = DateTime.Now;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceAsync(string functionKey, ReplaceFunctionRequest request, int actorUserId, CancellationToken ct = default)
    {
        if (string.Equals(functionKey, request.ReplacementFunctionKey, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Không thể thay thế chức năng bằng chính nó.");

        var oldFunction = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}'.");
        var replacement = await _db.Functions.SingleOrDefaultAsync(x => x.FunctionKey == request.ReplacementFunctionKey, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy chức năng thay thế '{request.ReplacementFunctionKey}'.");

        oldFunction.LifecycleStatus = "Replaced";
        oldFunction.ReplacementFunctionKey = replacement.FunctionKey;
        oldFunction.IsActive = false;
        oldFunction.ModifiedBy = actorUserId;
        oldFunction.ModifiedAt = DateTime.Now;

        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct);
        if (item != null)
        {
            item.LifecycleStatus = "Replaced";
            item.ReplacementFunctionKey = replacement.FunctionKey;
            item.ResolvedAt = DateTime.Now;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task IgnoreAsync(string functionKey, int actorUserId, CancellationToken ct = default)
    {
        var item = await _db.SecurityFunctionRegistry.SingleOrDefaultAsync(x => x.FunctionKey == functionKey, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy chức năng '{functionKey}'.");
        item.IsIgnored = true;
        item.LifecycleStatus = "Ignored";
        item.ResolvedAt = DateTime.Now;
        item.ModifiedBy = actorUserId;
        item.ModifiedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertFunctionAsync(SecurityFunctionUpsertRequest request, int actorUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FunctionKey)) throw new InvalidOperationException("FunctionKey không được để trống.");
        var entity = await _db.Functions.SingleOrDefaultAsync(x => x.Id == request.FunctionCode || x.FunctionKey == request.FunctionKey, ct);
        if (entity == null)
        {
            if (await _db.Functions.AnyAsync(x => x.FunctionCode == request.FunctionCode, ct))
                throw new InvalidOperationException($"FunctionCode {request.FunctionCode} đã tồn tại.");
            entity = new F03Function { FunctionCode = request.FunctionCode, CreatedBy = actorUserId };
            _db.Functions.Add(entity);
        }

        entity.FunctionCode = request.FunctionCode;
        entity.FunctionKey = request.FunctionKey.Trim();
        entity.FunctionName = request.FunctionName.Trim();
        entity.Detail = request.Detail?.Trim() ?? string.Empty;
        entity.ModuleCode = request.ModuleCode?.Trim();
        entity.ActionCode = request.ActionCode?.Trim();
        entity.ScopeCode = request.ScopeCode?.Trim();
        entity.DisplayOrder = request.DisplayOrder;
        entity.LifecycleStatus = "Active";
        entity.SourceType = "Manual";
        entity.IsActive = true;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteFunctionAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _db.Functions.Include(x => x.RoleFunctions).Include(x => x.UserFunctions).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Không tìm thấy chức năng.");
        if (entity.RoleFunctions.Count > 0 || entity.UserFunctions.Count > 0 || entity.LifecycleStatus != "Active")
        {
            entity.LifecycleStatus = "Retired";
            entity.IsActive = false;
            entity.ModifiedBy = actorUserId;
            entity.ModifiedAt = DateTime.Now;
        }
        else
        {
            _db.Functions.Remove(entity);
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertRoleAsync(SecurityRoleUpsertRequest request, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _db.Roles.SingleOrDefaultAsync(x => x.RoleCode == request.RoleCode, ct);
        if (entity == null)
        {
            entity = new F03Role { RoleCode = request.RoleCode, CreatedBy = actorUserId };
            _db.Roles.Add(entity);
        }
        if (entity.IsSystem && entity.RoleCode != request.RoleCode)
            throw new InvalidOperationException("Không được đổi mã của System Role.");
        entity.RoleName = request.RoleName.Trim();
        entity.Detail = request.Detail?.Trim();
        entity.IsSystem = entity.IsSystem || request.IsSystem;
        entity.IsActive = true;
        entity.ModifiedBy = actorUserId;
        entity.ModifiedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteRoleAsync(int id, int actorUserId, CancellationToken ct = default)
    {
        var entity = await _db.Roles.Include(x => x.RoleFunctions).Include(x => x.UserRoles).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Không tìm thấy Role.");
        if (entity.IsSystem || entity.RoleFunctions.Count > 0 || entity.UserRoles.Count > 0)
        {
            entity.IsActive = false;
            entity.ModifiedBy = actorUserId;
            entity.ModifiedAt = DateTime.Now;
        }
        else
        {
            _db.Roles.Remove(entity);
        }
        await _db.SaveChangesAsync(ct);
    }

    private static Dictionary<string, DiscoveredDefinition> DiscoverDefinitions()
    {
        var result = new Dictionary<string, DiscoveredDefinition>(StringComparer.OrdinalIgnoreCase);
        var type = typeof(SecurityFunctionCodes);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(int)) continue;
            var code = (int)(field.GetValue(null) ?? 0);
            if (code <= 0) continue;
            var attr = field.GetCustomAttribute<SecurityFunctionDefinitionAttribute>();
            var key = attr?.FunctionKey ?? ToFunctionKey(field.Name);
            var name = attr?.DisplayName ?? field.Name;
            var module = attr?.ModuleCode ?? GetModule(key);
            var action = attr?.ActionCode ?? GetAction(key);
            var scope = attr?.ScopeCode;
            var hash = Hash($"{key}|{code}|{name}|{module}|{action}|{scope}");
            result[key] = new DiscoveredDefinition(key, code, name, module, action, scope, "SecurityFunctionCodes", type.Assembly.GetName().Name, type.FullName, hash);
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
        {
            Type[] types;
            try { types = assembly.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(x => x != null).Cast<Type>().ToArray(); }
            foreach (var t in types)
            {
                foreach (var member in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    var attr = member.GetCustomAttribute<SecurityFunctionDefinitionAttribute>();
                    if (attr == null || string.IsNullOrWhiteSpace(attr.FunctionKey)) continue;
                    var code = 0;
                    if (member is FieldInfo fi && fi.FieldType == typeof(int)) code = (int)(fi.GetValue(null) ?? 0);
                    if (code == 0 && result.TryGetValue(attr.FunctionKey, out var existing)) code = existing.FunctionCode;
                    var name = attr.DisplayName ?? member.Name;
                    var module = attr.ModuleCode ?? GetModule(attr.FunctionKey);
                    var action = attr.ActionCode ?? GetAction(attr.FunctionKey);
                    var hash = Hash($"{attr.FunctionKey}|{code}|{name}|{module}|{action}|{attr.ScopeCode}");
                    result[attr.FunctionKey] = new DiscoveredDefinition(attr.FunctionKey, code, name, module, action, attr.ScopeCode, "Attribute", assembly.GetName().Name, t.FullName, hash);
                }
            }
        }
        return result;
    }

    private static string ToFunctionKey(string name)
    {
        var modules = new[] { "SecurityAccessChange", "UserManagement", "PublicInformation", "PublicForm", "WorkCalendar", "DepartmentStatus", "ApprovalPolicy", "HrmUserRoleRule", "EmailQueue", "EmailTemplate", "Equipment", "HrmSync", "Attendance", "Dashboard", "LeaveType", "Department", "Employee", "Approver", "OTLimit", "Execution", "Payroll", "Security", "Leave", "Trip", "OT" };
        var module = modules.OrderByDescending(x => x.Length).FirstOrDefault(name.StartsWith);
        if (module == null) return name;
        var suffix = name[module.Length..];
        return string.IsNullOrWhiteSpace(suffix) ? module : $"{module}.{suffix}";
    }

    private static string GetModule(string key) => key.Split('.', 2)[0];
    private static string? GetAction(string key) => key.Contains('.') ? key[(key.IndexOf('.') + 1)..] : null;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record DiscoveredDefinition(string FunctionKey, int FunctionCode, string DefinitionName, string? ModuleCode, string? ActionCode, string? ScopeCode, string SourceType, string? SourceAssembly, string? SourceTypeName, string DefinitionHash);
}
