using FVN_REGISTER.Contract.Util;


namespace FVN_REGISTER.Contract.Interfaces.Repositores
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 200;
        public DateTime ResponseTime { get; set; } = DateTime.Now;

        // --- BỔ SUNG CÁC HELPER PROPERTY ---
        public bool IsUnauthorized { get; set; }
        public bool IsForbidden => StatusCode == 403;
        public bool IsNotFound => StatusCode == 404;

        public static ApiResponse Ok(string? message = "Success", int statusCode = 200)
            => new ApiResponse { IsSuccess = true, Message = message ?? "Success", StatusCode = statusCode };

        public static ApiResponse Fail(string message, int statusCode = 400)
            => new ApiResponse { IsSuccess = false, Message = message, StatusCode = statusCode };

        public static ApiResponse FromResult(ServiceResult result)
            => result.IsSuccess ? Ok() : Fail(result.Message ?? "Error");
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = "Success")
            => new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message ?? "Success",
                Data = data,
                StatusCode = 200
            };

        public new static ApiResponse<T> Fail(string message, int statusCode = 400)
            => new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode,
                Data = default
            };

        public static ApiResponse<T> FromResult(ServiceResult<T> result)
        {
            return new ApiResponse<T>
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message,
                Data = result.Data,
                // 🔥 Đừng quên gán StatusCode mặc định nếu result không có status riêng
                StatusCode = result.IsSuccess ? 200 : 400
            };
        }
    }
}
