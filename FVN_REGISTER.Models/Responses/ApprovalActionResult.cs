


namespace FVN_REGISTER.Contract.Responses
{
    public sealed record ApprovalActionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public int SuccessCount { get; init; }
        public int TotalCount { get; init; }

        // Dùng IReadOnlyList là chuẩn rồi
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
        public IReadOnlyList<int> SuccessIds { get; init; } = Array.Empty<int>();
        public IReadOnlyList<int> FailedIds { get; init; } = Array.Empty<int>();

        // Constructor private để ép buộc dùng static factory hoặc Builder
        private ApprovalActionResult() { }

        public static ApprovalActionResult Ok(int successCount, int totalCount, string message) => new()
        {
            Success = true,
            SuccessCount = successCount,
            TotalCount = totalCount,
            Message = message
        };

        public static ApprovalActionResult Fail(string message, IEnumerable<string>? errors = null) => new()
        {
            Success = false,
            Message = message,
            Errors = errors?.ToList() ?? new List<string>()
        };

        // --- Builder Pattern: Tối ưu cho xử lý hàng loạt ---
        public class Builder
        {
            private int _total;
            private readonly List<int> _successIds = new();
            private readonly List<int> _failedIds = new();
            private readonly List<string> _errors = new();

            public Builder SetTotal(int total) { _total = total; return this; }
            public Builder AddSuccess(int id) { _successIds.Add(id); return this; }
            public Builder AddError(int id, string error) { _failedIds.Add(id); _errors.Add($"Đơn #{id}: {error}"); return this; }

            public ApprovalActionResult Build() => new()
            {
                TotalCount = _total,
                SuccessCount = _successIds.Count,
                Success = _errors.Count == 0,
                Message = _errors.Count == 0 ? "Xử lý thành công." : $"Lỗi xử lý {_errors.Count} đơn.",
                SuccessIds = _successIds,
                FailedIds = _failedIds,
                Errors = _errors
            };
        }
    }
}
