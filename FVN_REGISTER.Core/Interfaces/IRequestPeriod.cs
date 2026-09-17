

namespace FVN_REGISTER.Core.Interfaces
{
    public interface IRequestPeriod
    {
        //  dùng object hoặc DateTime nhưng sẽ casting ở lớp con
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
    }
}
