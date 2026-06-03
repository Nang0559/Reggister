using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.OT;

public class OTClientService : IOTClientService
{
    private readonly IHttpClientWithAuth _http;

    public OTClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<OTRequestDto>> CreateAsync(
        CreateOTRequestModel model, CancellationToken ct = default)
        => _http.PostAsync<OTRequestDto>("api/OT", model, ct);

    public Task<ApiResponse<OTValidationResult>> CheckHoursAsync(
        DateOnly otDate, List<OTEmployeeModel> employees, CancellationToken ct = default)
        => _http.PostAsync<OTValidationResult>("api/OT/check-hours",
            new { OTDate = otDate, Employees = employees }, ct);

    public Task<ApiResponse<object>> ApproveAsync(
        List<int> ids, int level, string? comment, CancellationToken ct = default)
        => _http.PostAsync<object>("api/OT/approve",
            new { Ids = ids, Level = level, Comment = comment }, ct);

    public Task<ApiResponse<object>> RejectAsync(
        List<int> ids, int level, string? comment, CancellationToken ct = default)
        => _http.PostAsync<object>("api/OT/reject",
            new { Ids = ids, Level = level, Comment = comment }, ct);

    public Task<ApiResponse<object>> CancelAsync(int id, CancellationToken ct = default)
        => _http.DeleteAsync<object>($"api/OT/{id}", ct);

    public Task<ApiResponse<List<OTRequestDto>>> GetMyOTAsync(
        int? year, CancellationToken ct = default)
        => _http.GetAsync<List<OTRequestDto>>(
            year.HasValue ? $"api/OT/my/{year}" : "api/OT/my", ct);

    public Task<ApiResponse<List<OTRequestDto>>> GetPendingAsync(CancellationToken ct = default)
        => _http.GetAsync<List<OTRequestDto>>("api/OT/pending", ct);

    public Task<ApiResponse<OTRequestDto>> GetByIdAsync(int id, CancellationToken ct = default)
        => _http.GetAsync<OTRequestDto>($"api/OT/{id}", ct);
    public Task<ApiResponse<List<EmployeeSelectDto>>> GetEmployeesByDeptAsync(
    string deptCode, CancellationToken ct = default)
    => _http.GetAsync<List<EmployeeSelectDto>>($"api/OT/employees/{deptCode}", ct);
}