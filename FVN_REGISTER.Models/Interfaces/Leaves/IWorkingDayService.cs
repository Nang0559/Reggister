

namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface IWorkingDayService
    {
        // Dùng Task vì có truy vấn DB (dù có cache nhưng khởi tạo vẫn cần async)
        Task<int> GetWorkingDaysAsync(DateTime from, DateTime to);
    }
}
