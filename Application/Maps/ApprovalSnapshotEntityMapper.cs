using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Application.Maps
{
    public static class ApprovalSnapshotEntityMapper
    {
        public static F03ApprovalSnapshot ToEntity(int requestId, ApprovalSnapshotDto dto)
        {
            return new F03ApprovalSnapshot
            {
                RequestId = requestId,
                RequestType = dto.ModuleName.ToString(),
                Steps = dto.Steps
                    .OrderBy(s => s.Level)
                    .Select(s => new F03ApprovalStepSnapshot
                    {
                        Level = s.Level,
                        RoleName = s.RoleName ?? string.Empty,
                        ApproverCode = s.ApproverCode ?? string.Empty,
                        ApproverName = s.ApproverName ?? string.Empty,
                        ApproverEmail = s.ApproverEmail ?? string.Empty,
                        IsRequired = s.IsRequired
                    })
                    .ToList()
            };
        }
    }
}
