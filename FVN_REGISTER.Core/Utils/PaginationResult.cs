namespace FVN_REGISTER.Core.Utils
{
    public class PaginationResult<T>
    {
        /// <summary>Danh sách item (trang hiện tại)</summary>
        public List<T> Data { get; set; } = new();

        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;

        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }

        public string? ErrorMessage { get; set; }
        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);

        public PaginationResult() { }

        public PaginationResult(List<T> data, int totalCount, int page, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        public static PaginationResult<T> Fail(string errorMessage)
            => new()
            {
                ErrorMessage = errorMessage,
                Data = new(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            };
    }
}
