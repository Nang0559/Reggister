using FVN_REGISTER.Contract.Dtos.MasterData;


namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    public sealed class LeaveCalendarDataDto
    {
        public SystemMasterDataDto MasterData { get; set; } = new();

        public int UserLevel { get; set; }
    }
}
