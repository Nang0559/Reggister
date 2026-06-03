using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Reports
{
    public interface IReportService
    {
        bool CanHandle(ReportType type);

        Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query,
            CurrentUser user,
            CancellationToken ct = default);

        Task<ServiceResult<byte[]>> ExportExcelAsync(
            ReportQueryDto query,
            CurrentUser user,
            CancellationToken ct = default);

        Task<ServiceResult<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(CurrentUser user, CancellationToken ct = default);
        Task<ServiceResult<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(string filterText, CurrentUser user, string? deptCode = null, CancellationToken ct = default);
    }
}
