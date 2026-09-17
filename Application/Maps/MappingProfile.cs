
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.Security;



namespace FVN_REGISTER.Application.Maps
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ReverseMap() giúp bạn tự động ánh xạ ngược lại (Entity <-> ViewModel)
            CreateMap<F03LeaveDay, LeaveRequestDto>().ReverseMap();

            CreateMap<F03User, UserIdentityDto>().ReverseMap();

            CreateMap<F03LeaveType, LeaveTypeDto>().ReverseMap();
        }
    }
}
