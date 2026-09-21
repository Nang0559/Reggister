using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Calendar;

public interface ICompanyHolidayManagementClientService
{
    Task<ApiResponse<List<CompanyHolidayDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<List<int>>> GetYearsAsync(CancellationToken ct = default);
    Task<ApiResponse<CompanyHolidayDto>> CreateAsync(CompanyHolidayDto model, CancellationToken ct = default);
    Task<ApiResponse<CompanyHolidayDto>> UpdateAsync(int id, CompanyHolidayDto model, CancellationToken ct = default);
    Task<ApiResponse<string>> DeleteAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<string>> CreateSundaysAsync(int year, CancellationToken ct = default);
    Task<ApiResponse<string>> ImportAsync(MultipartFormDataContent content, CancellationToken ct = default);
    Task<ApiResponse<byte[]>> DownloadTemplateAsync(CancellationToken ct = default);
}