using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Common
{
    /// <summary>
    /// Common query port for request modules such as Leave and Overtime.
    /// The implementation may use EF/SQL, but that detail stays in Infrastructure.
    /// </summary>
    public interface IRequestQueryService<TSummary, TBalance, TDto>
        where TSummary : class
        where TDto : class
    {
        Task<List<WidgetCounterDto>> GetMyWidgetsAsync(
            string employeeCode, CancellationToken ct = default);

        Task<List<TSummary>> GetRecentSummaryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default);

        Task<TBalance> GetSimpleBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default);

        Task<PaginationResult<TSummary>> GetPagedAsync(
            string? deptCode,
            ApprovalStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<ServiceResult<TDto>> GetFullDetailsAsync(
            int requestId, CancellationToken ct = default);

        Task<List<TDto>> GetDeptByDateAsync(
            string deptCode, DateTime date, CancellationToken ct = default);
    }
}
