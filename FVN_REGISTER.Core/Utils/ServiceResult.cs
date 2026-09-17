namespace FVN_REGISTER.Core.Utils
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public bool IsSuccess => Success;

        public static ServiceResult Ok(string? msg = null)
            => new() { Success = true, Message = msg };

        public static ServiceResult Fail(string msg)
            => new() { Success = false, Message = msg };

        public static ServiceResult<T> Ok<T>(T data, string? msg = null)
            => ServiceResult<T>.Ok(data, msg);
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string? msg = null)
            => new() { Success = true, Data = data, Message = msg };

        public new static ServiceResult<T> Fail(string msg)
            => new() { Success = false, Message = msg };
    }
}
