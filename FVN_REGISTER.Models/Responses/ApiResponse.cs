using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Contract.Responses;

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;

    public bool IsUnauthorized { get; set; }
    public bool IsForbidden => StatusCode == 403;
    public bool IsNotFound => StatusCode == 404;

    public static ApiResponse Ok(string? message = "Success", int statusCode = 200)
        => new() { IsSuccess = true, Message = message ?? "Success", StatusCode = statusCode };

    public static ApiResponse Fail(string message, int statusCode = 400)
        => new() { IsSuccess = false, Message = message, StatusCode = statusCode };

    public static ApiResponse FromResult(ServiceResult result)
        => result.IsSuccess ? Ok(result.Message) : Fail(result.Message ?? "Error");
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = "Success")
        => new()
        {
            IsSuccess = true,
            Message = message ?? "Success",
            Data = data,
            StatusCode = 200
        };

    public new static ApiResponse<T> Fail(string message, int statusCode = 400)
        => new()
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Data = default
        };

    public static ApiResponse<T> FromResult(ServiceResult<T> result)
        => new()
        {
            IsSuccess = result.IsSuccess,
            Message = result.Message ?? string.Empty,
            Data = result.Data,
            StatusCode = result.IsSuccess ? 200 : 400
        };
}
