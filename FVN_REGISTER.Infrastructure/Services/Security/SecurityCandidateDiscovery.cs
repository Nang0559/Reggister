using System.Reflection;
using FVN_REGISTER.Core.Attributes;
using FVN_REGISTER.Core.Entities.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

/// <summary>
/// Phát hiện các thao tác có khả năng là chức năng bảo mật từ điểm cuối HTTP và thành phần giao diện đã biên dịch.
/// Đây là cơ chế kiểm tra an toàn, không tự cấp quyền và không tự chặn nghiệp vụ.
/// </summary>
public sealed class SecurityCandidateDiscovery
{
    private static readonly string[] CandidateActionWords =
    [
        "Create", "Add", "Update", "Edit", "Delete", "Remove", "Approve", "Reject", "Cancel",
        "Submit", "Assign", "Handover", "Transfer", "Return", "Repair", "Import", "Export",
        "Upload", "Download", "Print", "Sync", "Calculate", "Recalculate", "Lock", "Unlock",
        "Reset", "Generate", "Send", "Publish", "Unpublish", "Archive", "Restore", "Execute"
    ];

    private readonly FVNWEBAPPContext _db;
    private readonly IEnumerable<EndpointDataSource> _endpointSources;

    public SecurityCandidateDiscovery(FVNWEBAPPContext db, IEnumerable<EndpointDataSource> endpointSources)
    {
        _db = db;
        _endpointSources = endpointSources;
    }

    public async Task<int> ScanAsync(CancellationToken cancellationToken = default)
    {
        var registry = await _db.SecurityFunctionRegistry
            .Where(x => x.SourceType is "ApiEndpointCandidate" or "UiActionCandidate")
            .ToDictionaryAsync(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.Now;

        foreach (var endpoint in _endpointSources.SelectMany(x => x.Endpoints).OfType<RouteEndpoint>())
        {
            var action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (action is null || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
                continue;

            var methodName = action.MethodInfo.Name;
            if (!LooksLikeBusinessAction(methodName))
                continue;
            if (action.MethodInfo.GetCustomAttribute<SecurityFunctionDefinitionAttribute>() is not null)
                continue;

            var controller = TrimControllerSuffix(action.ControllerTypeInfo.Name);
            var candidateKey = $"Candidate.Api.{controller}.{methodName}";
            seen.Add(candidateKey);
            UpsertCandidate(
                registry, candidateKey, $"Cần xác nhận chức năng {ToDisplayName(methodName)}", controller, methodName,
                "ApiEndpointCandidate", action.ControllerTypeInfo.Assembly.GetName().Name,
                $"{action.ControllerTypeInfo.FullName}.{methodName}",
                endpoint.RoutePattern.RawText ?? string.Empty, now);
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(x => x is not null).Cast<Type>().ToArray(); }

            foreach (var type in types.Where(x => !x.IsAbstract && typeof(ComponentBase).IsAssignableFrom(x)))
            {
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!LooksLikeBusinessAction(method.Name) || method.IsSpecialName)
                        continue;
                    if (method.GetCustomAttribute<SecurityFunctionDefinitionAttribute>() is not null)
                        continue;

                    var candidateKey = $"Candidate.Ui.{type.FullName}.{method.Name}";
                    seen.Add(candidateKey);
                    UpsertCandidate(
                        registry, candidateKey, $"Cần xác nhận thao tác giao diện {ToDisplayName(method.Name)}",
                        type.Name, method.Name, "UiActionCandidate", assembly.GetName().Name,
                        $"{type.FullName}.{method.Name}", type.FullName ?? type.Name, now);
                }
            }
        }

        foreach (var item in registry.Values.Where(x => !seen.Contains(x.FunctionKey) && x.LifecycleStatus == "PendingReview"))
        {
            item.LifecycleStatus = "PendingRetirement";
            item.ResolvedAt = null;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return seen.Count;
    }

    private void UpsertCandidate(
        IDictionary<string, F03SecurityFunctionRegistryItem> registry,
        string key,
        string definition,
        string module,
        string action,
        string sourceType,
        string? sourceAssembly,
        string sourceTypeName,
        string evidence,
        DateTime now)
    {
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(key + "|" + evidence))).ToLowerInvariant();

        if (!registry.TryGetValue(key, out var item))
        {
            item = new F03SecurityFunctionRegistryItem
            {
                FunctionKey = key,
                FunctionCode = 0,
                DefinitionName = definition,
                ModuleCode = module,
                ActionCode = action,
                ScopeCode = "Review",
                LifecycleStatus = "PendingReview",
                SourceType = sourceType,
                SourceAssembly = sourceAssembly,
                SourceTypeName = sourceTypeName,
                DefinitionHash = hash,
                FirstDiscoveredAt = now,
                LastSeenAt = now,
                IsIgnored = false
            };
            _db.SecurityFunctionRegistry.Add(item);
            registry[key] = item;
            return;
        }

        item.LastSeenAt = now;
        item.DefinitionHash = hash;
        item.DefinitionName = definition;
        item.SourceAssembly = sourceAssembly;
        item.SourceTypeName = sourceTypeName;
        if (item.LifecycleStatus is not ("Ignored" or "Retired" or "Replaced"))
            item.LifecycleStatus = "PendingReview";
    }

    private static string TrimControllerSuffix(string name) =>
        name.EndsWith("Controller", StringComparison.Ordinal) ? name[..^10] : name;

    private static bool LooksLikeBusinessAction(string methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName)) return false;
        if (methodName.EndsWith("Async", StringComparison.Ordinal)) methodName = methodName[..^5];
        return CandidateActionWords.Any(word =>
            string.Equals(methodName, word, StringComparison.OrdinalIgnoreCase) ||
            methodName.StartsWith(word, StringComparison.OrdinalIgnoreCase));
    }

    private static string ToDisplayName(string methodName)
    {
        if (methodName.EndsWith("Async", StringComparison.Ordinal)) methodName = methodName[..^5];
        return methodName switch
        {
            "Create" or "Add" => "thêm mới",
            "Update" or "Edit" => "cập nhật",
            "Delete" or "Remove" => "xóa",
            "Approve" => "phê duyệt",
            "Reject" => "từ chối",
            "Cancel" => "hủy",
            "Submit" => "gửi xử lý",
            "Assign" or "Handover" => "bàn giao",
            "Transfer" => "điều chuyển",
            "Return" => "thu hồi",
            "Repair" => "lập phiếu sửa chữa",
            "Import" => "nhập dữ liệu",
            "Export" => "xuất dữ liệu",
            "Upload" => "tải lên",
            "Download" => "tải xuống",
            "Print" => "in",
            "Sync" => "đồng bộ",
            "Calculate" or "Recalculate" => "tính toán",
            "Lock" => "khóa",
            "Unlock" => "mở khóa",
            "Reset" => "đặt lại",
            "Generate" => "tạo dữ liệu",
            "Send" => "gửi",
            "Publish" => "công bố",
            "Unpublish" => "hủy công bố",
            "Archive" => "lưu trữ",
            "Restore" => "khôi phục",
            "Execute" => "thực hiện",
            _ => methodName
        };
    }
}
