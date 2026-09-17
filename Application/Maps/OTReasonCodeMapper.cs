using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Core.Entities.OT;


namespace FVN_REGISTER.Application.Maps
{
    public static class OTReasonCodeMapper
    {
        public static OTReasonCodeDto ToDto(this F03OTReasonCode e) => new()
        {
            Id = e.Id,
            ReasonCode = e.ReasonCode,
            DisplayName = e.DisplayName,
            Description = e.Description,
            DisplayOrder = e.DisplayOrder,
            IsActive = e.IsActive ?? false
        };

        public static F03OTReasonCode ToEntity(this OTReasonCodeUpsertDto m, int currentUserId) => new()
        {
            ReasonCode = m.ReasonCode,
            DisplayName = m.DisplayName,
            Description = m.Description,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.Now,
            ModifiedBy = currentUserId,
            ModifiedAt = DateTime.Now
        };

        public static void ApplyTo(this OTReasonCodeUpsertDto m, F03OTReasonCode entity, int currentUserId)
        {
            entity.ReasonCode = m.ReasonCode;
            entity.DisplayName = m.DisplayName;
            entity.Description = m.Description;
            entity.DisplayOrder = m.DisplayOrder;
            entity.IsActive = m.IsActive;
        }
    }
}
