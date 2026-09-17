namespace FVN_REGISTER.Application.Models
{
    /// <summary>
    /// Application-level query options shared by query use cases.
    /// Persistence-specific repository options belong to Infrastructure.
    /// </summary>
    public sealed class QueryOptionsDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = string.Empty;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
