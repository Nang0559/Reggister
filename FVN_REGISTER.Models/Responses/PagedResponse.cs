

namespace FVN_REGISTER.Contract.Responses
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new(); // Đổi Data thành Items để tránh trùng tên với ApiResponse.Data
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;
    }
}
