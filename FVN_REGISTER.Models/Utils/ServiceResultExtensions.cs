

namespace FVN_REGISTER.Core.Utils
{
    public static class ServiceResultExtensions
    {
        /// <summary>
        /// Ép ServiceResult&lt;T&gt; về ServiceResult non-generic (bỏ Data), 
        /// dùng khi orchestrator cần đồng bộ kiểu trả về cho HandleResult() hoặc gộp nhiều bước.
        /// </summary>
        public static ServiceResult ToBase<T>(this ServiceResult<T> result)
            => result.IsSuccess
                ? ServiceResult.Ok(result.Message)
                : ServiceResult.Fail(result.Message ?? "Lỗi không xác định");
    }
}
