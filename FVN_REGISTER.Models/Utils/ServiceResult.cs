namespace FVN_REGISTER.Contract.Utils;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public bool IsSuccess => Success;

    public static ServiceResult Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ServiceResult Fail(string message)
        => new() { Success = false, Message = message };

    public static ServiceResult<T> Ok<T>(T data, string? message = null)
        => ServiceResult<T>.Ok(data, message);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public new static ServiceResult<T> Fail(string message)
        => new() { Success = false, Message = message };
}
