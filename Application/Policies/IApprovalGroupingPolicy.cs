using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Policies
{
    /// <summary>
    /// Thuần logic — gộp danh sách chờ duyệt đa module thành các nhóm theo
    /// (Module, Level) để hiển thị trên Inbox. Không I/O.
    /// LƯU Ý: PendingApprovalGroupDto không có field Kind — mọi item trong
    /// cùng 1 group được đảm bảo cùng Module vì key gộp là (Module, Level).
    /// Consumer (UI) lấy module qua group.Requests.First().Kind.
    /// </summary>
    public interface IApprovalGroupingPolicy
    {
        List<PendingApprovalGroupDto> BuildGroups(
            IReadOnlyDictionary<RequestModule, List<PendingApprovalItemDto>> byModule);
    }
}
