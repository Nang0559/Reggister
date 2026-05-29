using AutoMapper;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Maps
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ReverseMap() giúp bạn tự động ánh xạ ngược lại (Entity <-> ViewModel)
            CreateMap<F03leaveDay, LeaveDaysViewModel>().ReverseMap();

            CreateMap<F03user, UserAccountViewModel>().ReverseMap();

            CreateMap<VF03leaveDaysApprover, ApproverViewModel>().ReverseMap();

            CreateMap<F03leaveType, LeaveTypeViewModel>().ReverseMap();
        }
    }
}
