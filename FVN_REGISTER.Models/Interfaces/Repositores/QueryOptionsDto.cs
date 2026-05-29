

namespace FVN_REGISTER.Contract.Interfaces.Repositores
{
    public class QueryOptionsDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = string.Empty;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
