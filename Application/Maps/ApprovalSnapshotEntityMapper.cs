using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;


namespace FVN_REGISTER.Application.Maps
{
    /// <summary>
    /// Chuyển ApprovalSnapshotDto (Application) sang entity F03ApprovalSnapshot
    /// để persist. Đặt ở Infrastructure vì đây là mapping DTO -> EF Entity,
    /// đúng nguyên tắc Application không biết chi tiết Infrastructure.
    /// </summary>
    public static class ApprovalSnapshotEntityMapper
    {
        public static F03ApprovalSnapshot ToEntity(int requestId, ApprovalSnapshotDto dto)
        {
            return new F03ApprovalSnapshot
            {
                RequestId = requestId,
                RequestType = dto.ModuleName,
                Steps = dto.Steps
                    .OrderBy(s => s.Level)
                    .Select(s => new F03ApprovalStepSnapshot
                    {
                        Level = s.Level,
                        RoleName = s.RoleName,
                        ApproverCode = s.ApproverCode,
                        ApproverName = s.ApproverName,
                        ApproverEmail = s.ApproverEmail,   // cần thêm field này vào entity
                        IsRequired = s.IsRequired
                    })
                    .ToList()
            };
        }
    }
}
