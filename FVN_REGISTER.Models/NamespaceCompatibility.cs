// Transitional compatibility for stale result-type namespaces during the DTO migration.
namespace FVN_REGISTER.Contract.Models { }

namespace FVN_REGISTER.Contract.Utils
{
    public class ServiceResult : FVN_REGISTER.Core.Utils.ServiceResult
    {
        public new static ServiceResult Ok(string? msg = null)
            => new() { Success = true, Message = msg };

        public new static ServiceResult Fail(string msg)
            => new() { Success = false, Message = msg };

        public static implicit operator ServiceResult(FVN_REGISTER.Core.Utils.ServiceResult value)
            => new() { Success = value.Success, Message = value.Message };
    }

    public class ServiceResult<T> : FVN_REGISTER.Core.Utils.ServiceResult<T>
    {
        public new static ServiceResult<T> Ok(T data, string? msg = null)
            => new() { Success = true, Data = data, Message = msg };

        public new static ServiceResult<T> Fail(string msg)
            => new() { Success = false, Message = msg };

        public static implicit operator ServiceResult<T>(FVN_REGISTER.Core.Utils.ServiceResult<T> value)
            => new() { Success = value.Success, Data = value.Data, Message = value.Message };
    }

    public class PaginationResult<T> : FVN_REGISTER.Core.Utils.PaginationResult<T>
    {
        public PaginationResult() { }

        public PaginationResult(List<T> data, int totalCount, int page, int pageSize)
            : base(data, totalCount, page, pageSize) { }

        public static implicit operator PaginationResult<T>(FVN_REGISTER.Core.Utils.PaginationResult<T> value)
            => new(value.Data, value.TotalCount, value.Page, value.PageSize)
            {
                SortBy = value.SortBy,
                SortDirection = value.SortDirection,
                ErrorMessage = value.ErrorMessage
            };
    }
}
