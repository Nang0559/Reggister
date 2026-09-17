namespace FVN_REGISTER.Core.Utils
{
    public static class ServiceResultExtensions
    {
        public static ServiceResult ToBase<T>(this ServiceResult<T> result)
            => result.IsSuccess
                ? ServiceResult.Ok(result.Message)
                : ServiceResult.Fail(result.Message ?? "Lỗi không xác định");
    }
}
