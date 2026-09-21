using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Calendar;

public sealed class CompanyHolidayManagementClientService : ICompanyHolidayManagementClientService
{
    readonly IHttpClientWithAuth _http;

    public CompanyHolidayManagementClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<List<CompanyHolidayDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetAsync<List<CompanyHolidayDto>>("api/company-holidays", ct);

    public Task<ApiResponse<List<int>>> GetYearsAsync(CancellationToken ct = default)
        => _http.GetAsync<List<int>>("api/company-holidays/years", ct);

    public Task<ApiResponse<CompanyHolidayDto>> CreateAsync(
        CompanyHolidayDto model,
        CancellationToken ct = default)
        => _http.PostAsync<CompanyHolidayDto>("api/company-holidays", model, ct);

    public Task<ApiResponse<CompanyHolidayDto>> UpdateAsync(
        int id,
        CompanyHolidayDto model,
        CancellationToken ct = default)
        => _http.PutAsync<CompanyHolidayDto>($"api/company-holidays/{id}", model, ct);

    public Task<ApiResponse<string>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteAsync<string>($"api/company-holidays/{id}", ct);

    public Task<ApiResponse<string>> CreateSundaysAsync(
        int year,
        CancellationToken ct = default)
        => _http.PostAsync<string>(
            $"api/company-holidays/sundays/{year}",
            new { },
            ct);

    public Task<ApiResponse<string>> ImportAsync(
        MultipartFormDataContent content,
        CancellationToken ct = default)
        => _http.PostMultipartAsync<string>("api/company-holidays/import", content, ct);

    public Task<ApiResponse<byte[]>> DownloadTemplateAsync(CancellationToken ct = default)
        => _http.GetFileAsync("api/company-holidays/template", ct);
}