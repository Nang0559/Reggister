


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Reports
{
    public interface IReportDispatcher
    {
        Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default);
        Task<ServiceResult<byte[]>> ExportExcelAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default);
        Task<ServiceResult<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(
            UserIdentityDto user, CancellationToken ct = default);
        Task<ServiceResult<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(
            string filterText, UserIdentityDto user,
            string? deptCode = null, CancellationToken ct = default);
    }
}
