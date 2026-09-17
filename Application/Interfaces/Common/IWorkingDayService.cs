namespace FVN_REGISTER.Application.Interfaces.Common
{
    public interface IWorkingDayService
    {
        // Dùng Task vì có truy vấn DB (dù có cache nhưng khởi tạo vẫn cần async)
        Task<int> GetWorkingDaysAsync(DateTime from, DateTime to);

        // Thêm hàm tính số giờ làm việc thực tế
        Task<double> GetWorkingHoursAsync(DateTime from, DateTime to);
    }
}
