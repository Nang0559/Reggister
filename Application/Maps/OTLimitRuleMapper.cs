using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Core.Entities.OT;


namespace FVN_REGISTER.Application.Maps
{
    public static class OTLimitRuleMapper
    {
        public static OTLimitRuleDto ToDto(this F03OTLimitRule e) => new()
        {
            Id = e.Id,
            LimitType = e.LimitType,
            LimitValue = e.LimitValue,
            PositionCode = e.PositionCode,
            DeptCode = e.DeptCode,
            LimitHours = e.LimitHours,
            Description = e.Description
        };

        public static F03OTLimitRule ToEntity(this OTLimitRuleUpsertDto m, int currentUserId) => new()
        {
            LimitType = m.LimitType,
            LimitValue = m.LimitValue,
            PositionCode = m.PositionCode,
            DeptCode = m.DeptCode,
            LimitHours = m.LimitHours,
            Description = m.Description,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.Now,
            ModifiedBy = currentUserId,
            ModifiedAt = DateTime.Now
        };

        public static void ApplyTo(this OTLimitRuleUpsertDto m, F03OTLimitRule entity, int currentUserId)
        {
            entity.LimitType = m.LimitType;
            entity.LimitValue = m.LimitValue;
            entity.PositionCode = m.PositionCode;
            entity.DeptCode = m.DeptCode;
            entity.LimitHours = m.LimitHours;
            entity.Description = m.Description;
        }
    }
}
