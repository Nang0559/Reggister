
namespace FVN_REGISTER.Application.Interfaces.HrmSync
{
    /// <summary>
    /// Đọc dữ liệu thô trực tiếp từ HRM DB (cross-database, cùng instance với FVNWEBAPPDB).
    /// Mỗi entity type có 1 Reader riêng, 1-1 với Importer tương ứng — KHÔNG có marker/Resolver
    /// vì Reader chỉ được gọi bởi đúng 1 Importer, không cần resolve động như Job/Importer.
    /// </summary>
    public interface IHrmSourceReader<TSourceRow>
        where TSourceRow : class
    {
        /// <summary>
        /// Đọc toàn bộ snapshot hiện tại từ bảng nguồn HRM.
        /// Luôn full-pull (không incremental) vì HRM không đảm bảo có cột theo dõi thay đổi đáng tin cậy —
        /// việc so sánh Insert/Update/Delete được tính ở tầng Importer dựa trên diff với bảng đích.
        /// </summary>
        Task<List<TSourceRow>> ReadAllAsync(CancellationToken ct = default);
    }
}
