using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class WorkerRunStateExtensions
    {
        public static string ToDisplayName(this WorkerRunState state) => state switch
        {
            WorkerRunState.Waiting => "Đang chờ đến giờ chạy",
            WorkerRunState.Running => "Đang đồng bộ dữ liệu chấm công vào staging...",
            WorkerRunState.Error => "Lần chạy trước gặp lỗi",
            WorkerRunState.Stopped => "Worker đã dừng",
            _ => "Không xác định"
        };
    }
}
