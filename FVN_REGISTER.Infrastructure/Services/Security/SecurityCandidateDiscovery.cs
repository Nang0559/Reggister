using System.Reflection;
using FVN_REGISTER.Core.Attributes;
using FVN_REGISTER.Core.Entities.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

/// <summary>
/// Phát hiện các điểm cuối HTTP có khả năng là chức năng bảo mật nhưng chưa có khai báo chức năng.
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
        var functions = await _db.Functions.AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.FunctionKey))
            .Select(x => x.FunctionKey!)
            .ToListAsync(cancellationToken);
        var known = functions.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var registry = await _db.SecurityFunctionRegistry
            .Where(x => x.SourceType == "ApiEndpointCandidate")
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

            var explicitDefinition = action.MethodInfo.GetCustomAttribute<SecurityFunctionDefinitionAttribute>();
            if (explicitDefinition is not null)
                continue;

            var controller = action.ControllerTypeInfo.Name.EndsWith("Controller", StringComparison.Ordinal)
                ? action.ControllerTypeInfo.Name[..^10]
                : action.ControllerTypeInfo.Name;
            var candidateKey = $"Candidate.{controller}.{methodName}";
            seen.Add(candidateKey);

            var route = endpoint.RoutePattern.RawText ?? string.Empty;
            var definition = $"Cần xác nhận chức năng {ToDisplayName(methodName)}";
            var sourceName = $"{action.ControllerTypeInfo.FullName}.{methodName}";
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(candidateKey + "|" + route))).ToLowerInvariant();

            if (!registry.TryGetValue(candidateKey, out var item))
            {
                item = new F03SecurityFunctionRegistryItem
                {
                    FunctionKey = candidateKey,
                    FunctionCode = 0,
                    DefinitionName = definition,
                    ModuleCode = controller,
                    ActionCode = methodName,
                    ScopeCode = "Review",
                    LifecycleStatus = "PendingReview",
                    SourceType = "ApiEndpointCandidate",
                    SourceAssembly = action.ControllerTypeInfo.Assembly.GetName().Name,
                    SourceTypeName = sourceName,
                    DefinitionHash = hash,
                    FirstDiscoveredAt = now,
                    LastSeenAt = now,
                    IsIgnored = false
                };
                _db.SecurityFunctionRegistry.Add(item);
                registry[candidateKey] = item;
            }
            else
            {
                item.LastSeenAt = now;
                item.DefinitionHash = hash;
                item.DefinitionName = definition;
                item.SourceAssembly = action.ControllerTypeInfo.Assembly.GetName().Name;
                item.SourceTypeName = sourceName;
                if (item.LifecycleStatus is not ("Ignored" or "Retired" or "Replaced"))
                    item.LifecycleStatus = "PendingReview";
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
