


using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Maps
{
    /// <summary>
    /// Chuyển kết quả resolve approver (F03Approver + Required) thành
    /// List&lt;ApprovalStepSnapshotDto&gt; — THUẦN DTO, không tạo entity ở đây.
    /// Việc persist entity (F03ApprovalSnapshot/F03ApprovalStepSnapshot) thuộc
    /// về IApprovalEngine (Infrastructure), KHÔNG thuộc Application.
    /// </summary>
    public static class ApprovalSubjectMapper
    {
        public static List<ApprovalStepSnapshotDto> BuildStepDtos(
     IEnumerable<(F03Approver Approver, bool Required)> resolvedSteps)
        {
            return resolvedSteps
                .OrderBy(x => x.Approver.Level)
                .Select(x => new ApprovalStepSnapshotDto(
                    Level: x.Approver.Level,
                    LevelName: $"Cấp {x.Approver.Level}",
                    RoleName: x.Approver.RoleName,
                    ApproverCode: x.Approver.ApproverCode,
                    ApproverName: x.Approver.ApproverName,
                    ApproverEmail: x.Approver.ApproverEmail,
                    IsRequired: x.Required
                ))
                .ToList();
        }
    }
}

