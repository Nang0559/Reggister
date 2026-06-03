using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Contract.Interfaces.Repositores;


namespace FVN_REGISTER.Shared.Services.Leaves
{
    // FVN_REGISTER.Shared/Services/Reports/IReportClientService.cs
    public interface IReportClientService
    {
        Task<ApiResponse<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, CancellationToken ct = default);

        Task<ApiResponse<byte[]>> ExportExcelAsync(
            ReportQueryDto query, CancellationToken ct = default);
        Task<ApiResponse<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(string text, CancellationToken ct = default);
        Task<ApiResponse<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(CancellationToken ct = default);
    }
}
