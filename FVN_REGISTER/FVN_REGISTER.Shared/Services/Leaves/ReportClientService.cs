using FVN_REGISTER.Contract.Dtos.Reports;

using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;


namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class ReportClientService : IReportClientService
    {
        private readonly IHttpClientWithAuth _http;

        // BỔ SUNG: Hàm khởi tạo để DI có thể inject IHttpClientWithAuth vào
        public ReportClientService(IHttpClientWithAuth http)
        {
            _http = http;
        }

        public async Task<ApiResponse<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, CancellationToken ct = default)
            => await _http.PostAsync<ReportResultDto>("api/report", query, ct);

        public async Task<ApiResponse<byte[]>> ExportExcelAsync(ReportQueryDto query, CancellationToken ct = default)
        {
            return await _http.PostFileAsync("api/report/export/excel", query, ct);
        }

        public async Task<ApiResponse<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(CancellationToken ct = default)
    => await _http.GetAsync<List<KeyValuePair<string, string>>>("api/report/lookup/departments", ct);

        public async Task<ApiResponse<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(string text, CancellationToken ct = default)
            => await _http.GetAsync<List<KeyValuePair<string, string>>>($"api/report/lookup/employees?text={System.Net.WebUtility.UrlEncode(text)}", ct);
    }
}
