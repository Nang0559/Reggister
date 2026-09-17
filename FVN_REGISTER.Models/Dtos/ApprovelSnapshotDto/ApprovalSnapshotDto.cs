

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto
{
    public sealed record ApprovalSnapshotDto(
    int RequestId,
    RequestModule ModuleName,
    DateTime CapturedAt, // Thời điểm snapshot được tạo
    IReadOnlyList<ApprovalStepSnapshotDto> Steps // Danh sách các bước duyệt đã "đóng băng"
);
}
