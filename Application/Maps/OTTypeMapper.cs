using FVN_REGISTER.Contract.Dtos.OTTypeDtos;
using FVN_REGISTER.Core.Entities.OT;


namespace FVN_REGISTER.Application.Maps
{
    public static class OTTypeMapper
    {
        public static OTTypeDto ToDto(this F03OTType e) => new()
        {
            Id = e.Id,
            OTTypeCode = e.OTTypeCode,
            OTTypeName = e.OTTypeName,
            OTTypeName2 = e.OTTypeName2,
            RateMultiplier = e.RateMultiplier,
            HRMCode = e.HRMCode,
            IsActive = e.IsActive??false
        };

        public static F03OTType ToEntity(this OTTypeUpsertDto m, int currentUserId) => new()
        {
            OTTypeCode = m.OTTypeCode,
            OTTypeName = m.OTTypeName,
            OTTypeName2 = m.OTTypeName2,
            RateMultiplier = m.RateMultiplier,
            IsActive = m.IsActive,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.Now,
            ModifiedBy = currentUserId,
            ModifiedAt = DateTime.Now
            // HRMCode KHÔNG set từ Admin — dành riêng cho HrmSyncJob nếu sau này HRM có nguồn.
        };

        public static void ApplyTo(this OTTypeUpsertDto m, F03OTType entity, int currentUserId)
        {
            entity.OTTypeCode = m.OTTypeCode;
            entity.OTTypeName = m.OTTypeName;
            entity.OTTypeName2 = m.OTTypeName2;
            entity.RateMultiplier = m.RateMultiplier;
            entity.IsActive = m.IsActive;
            // HRMCode không đổi qua Admin upsert.
        }
    }
}
