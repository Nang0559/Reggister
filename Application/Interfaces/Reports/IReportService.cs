


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Interfaces.Reports
{
    public interface IReportService
    {
        bool CanHandle(ReportType type);

        Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query,
            UserIdentityDto user,
            CancellationToken ct = default);

        Task<ServiceResult<byte[]>> ExportExcelAsync(
            ReportQueryDto query,
            UserIdentityDto user,
            CancellationToken ct = default);

        // Lookups — mỗi service tự implement phần của mình
        Task<ServiceResult<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(
            UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(
            string filterText, UserIdentityDto user,
            string? deptCode = null, CancellationToken ct = default);
    }
}
