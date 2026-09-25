using System.Reflection;
using FVN_REGISTER.Contract.Dtos.Security;

namespace FVN_REGISTER.Web.Security;

/// <summary>
/// Phát hiện các thao tác giao diện có khả năng là chức năng nghiệp vụ.
/// Kết quả chỉ là candidate để Security Registry xác nhận, không tự cấp quyền.
/// </summary>
public static class SecurityDiscoveryManifestBuilder
{
    private static readonly string[] ActionTokens =
    [
        "Create", "Add", "Edit", "Update", "Delete", "Remove",
        "Approve", "Reject", "Cancel", "Submit", "Save",
        "Import", "Export", "Excel", "Print", "Download", "Upload",
        "Handover", "Transfer", "Return", "Repair", "Sync",
        "Calculate", "Recalculate", "Lock", "Unlock", "Publish",
        "Archive", "Restore", "Reset"
    ];

    public static IReadOnlyList<WebSecurityFunctionCandidateDto> Discover(IEnumerable<Assembly> assemblies)
    {
        var result = new List<WebSecurityFunctionCandidateDto>();

        foreach (var assembly in assemblies.Distinct())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(static x => x is not null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (method.IsSpecialName || method.IsConstructor)
                        continue;

                    var token = ActionTokens.FirstOrDefault(x => method.Name.Contains(x, StringComparison.OrdinalIgnoreCase));
                    if (token is null)
                        continue;

                    var module = ResolveModule(type);
                    var action = token;
                    var functionKey = $"{module}.{action}";
                    var displayName = ToDisplayName(action);

                    result.Add(new WebSecurityFunctionCandidateDto(
                        functionKey,
                        module,
                        action,
                        assembly.GetName().Name ?? "Web",
                        "UI",
                        null,
                        type.FullName,
                        displayName,
                        $"Thao tác {displayName.ToLowerInvariant()} của phân hệ {module}."));
                }
            }
        }

        return result
            .GroupBy(x => new { x.FunctionKey, x.Source, x.Component })
            .Select(x => x.First())
            .OrderBy(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ResolveModule(Type type)
    {
        var name = type.Namespace?.Split('.').LastOrDefault();
        return string.IsNullOrWhiteSpace(name) ? "Web" : name;
    }

    private static string ToDisplayName(string action) => action switch
    {
        "Create" or "Add" => "Thêm",
        "Edit" or "Update" => "Cập nhật",
        "Delete" or "Remove" => "Xóa",
        "Approve" => "Phê duyệt",
        "Reject" => "Từ chối",
        "Cancel" => "Hủy",
        "Submit" => "Gửi xử lý",
        "Import" => "Nhập dữ liệu",
        "Export" or "Excel" => "Xuất dữ liệu",
        "Print" => "In",
        "Download" => "Tải xuống",
        "Upload" => "Tải lên",
        "Handover" => "Bàn giao",
        "Transfer" => "Điều chuyển",
        "Return" => "Thu hồi",
        "Repair" => "Sửa chữa",
        "Sync" => "Đồng bộ",
        "Calculate" or "Recalculate" => "Tính toán",
        "Lock" => "Khóa",
        "Unlock" => "Mở khóa",
        "Publish" => "Công bố",
        "Archive" => "Lưu trữ",
        "Restore" => "Khôi phục",
        "Reset" => "Đặt lại",
        _ => action
    };
}
