using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
namespace FVN_REGISTER.Shared.Services.Calendar;
public sealed class WorkYearManagementClientService:IWorkYearManagementClientService
{
 private readonly IHttpClientWithAuth _http; public WorkYearManagementClientService(IHttpClientWithAuth http)=>_http=http;
 public Task<ApiResponse<List<WorkYearDto>>> GetAllAsync(CancellationToken ct=default)=>_http.GetAsync<List<WorkYearDto>>("api/work-years",ct);
 public Task<ApiResponse<WorkYearDto>> CreateAsync(WorkYearDto m,CancellationToken ct=default)=>_http.PostAsync<WorkYearDto>("api/work-years",m,ct);
 public Task<ApiResponse<WorkYearDto>> UpdateAsync(int id,WorkYearDto m,CancellationToken ct=default)=>_http.PutAsync<WorkYearDto>($"api/work-years/{id}",m,ct);
 public Task<ApiResponse<string>> SetActiveAsync(int id,bool active,CancellationToken ct=default)=>_http.PostAsync<string>($"api/work-years/{id}/active?active={active}",new{},ct);
}