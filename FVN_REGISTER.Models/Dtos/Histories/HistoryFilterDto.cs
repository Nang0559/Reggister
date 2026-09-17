


using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Histories
{
    public class HistoryFilterDto
    {
        public RequestModule Kind { get; set; } = RequestModule.Leave;
        public int? Year { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchText { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
