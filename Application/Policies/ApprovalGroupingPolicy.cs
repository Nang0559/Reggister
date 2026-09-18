using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Policies
{
    public class ApprovalGroupingPolicy : IApprovalGroupingPolicy
    {
        public List<PendingApprovalGroupDto> BuildGroups(
            IReadOnlyDictionary<RequestModule, List<PendingApprovalItemDto>> byModule)
        {
            if (byModule == null || byModule.Count == 0)
                return new List<PendingApprovalGroupDto>();

            // Chỉ lấy item mà approver hiện tại thực sự được phép duyệt.
            // CanApprove do Engine gán sẵn (dựa trên quyền), Policy không tự suy luận quyền.
            var flat = byModule
                .SelectMany(kv => kv.Value)
                .Where(item => item.CanApprove)
                .ToList();

            var groups = flat
                .GroupBy(item => new
                {
                    item.Kind,
                    ApproverLevel = GetCurrentApprovalLevel(item)
                })
                .Where(g => g.Key.ApproverLevel > 0)
                .Select(g => new PendingApprovalGroupDto
                {
                    RequestType = g.Key.Kind,
                    ApproverLevel = g.Key.ApproverLevel,
                    Count = g.Count(),
                    OverriddenCount = g.Count(i => i.ApprovalSteps.Any(s => s.IsOverriddenByAdmin)),
                    Requests = SortItems(g.ToList())
                })
                .ToList();

            return SortGroups(groups);
        }

        private static int GetCurrentApprovalLevel(PendingApprovalItemDto item)
            => item.ApprovalSteps
                .OrderBy(s => s.Level)
                .FirstOrDefault(s => s.IsRequired && s.IsApproved == null)?.Level ?? 0;

        // Module nào lên trước — quyết định nghiệp vụ, tách riêng để dễ đổi khi thêm module.
        private static List<PendingApprovalGroupDto> SortGroups(List<PendingApprovalGroupDto> groups)
            => groups
                .OrderBy(g => ModulePriority(g.RequestType))
                .ThenBy(g => g.ApproverLevel)
                .ToList();

        private static int ModulePriority(RequestModule module) => module switch
        {
            RequestModule.Leave => 0,
            RequestModule.Overtime => 1,
            RequestModule.Trip => 2,
            _ => 99
        };

        // Ưu tiên đơn sắp/đã quá hạn duyệt lên đầu (TimeoutDays càng nhỏ càng gấp),
        // rồi đến đơn nộp lâu nhất (SubmittedAt càng cũ càng ưu tiên),
        // cuối cùng fallback RequestId để thứ tự ổn định giữa các lần load khi 2 tiêu chí trên bằng nhau
        // (tránh UI nhảy lung tung khi TimeoutDays/SubmittedAt trùng).
        private static List<PendingApprovalItemDto> SortItems(List<PendingApprovalItemDto> items)
            => items
                .OrderBy(i => i.TimeoutDays ?? int.MaxValue)
                .ThenBy(i => i.SubmittedAt ?? DateTime.MaxValue)
                .ThenBy(i => i.RequestId)
                .ToList();
    }
}
