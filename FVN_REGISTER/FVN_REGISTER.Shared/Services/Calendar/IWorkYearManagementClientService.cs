using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Responses;
namespace FVN_REGISTER.Shared.Services.Calendar;
public interface IWorkYearManagementClientService
{
 Task<ApiResponse<List<WorkYearDto>>> GetAllAsync(CancellationToken ct=default);
 Task<ApiResponse<WorkYearDto>> CreateAsync(WorkYearDto model,CancellationToken ct=default);
 Task<ApiResponse<WorkYearDto>> UpdateAsync(int id,WorkYearDto model,CancellationToken ct=default);
 Task<ApiResponse<string>> SetActiveAsync(int id,bool active,CancellationToken ct=default);
}