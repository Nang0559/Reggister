using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface IWorkYearManagementService
{
    Task<List<WorkYearDto>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<WorkYearDto>> CreateAsync(WorkYearDto model, int userId, CancellationToken ct = default);
    Task<ServiceResult<WorkYearDto>> UpdateAsync(WorkYearDto model, int userId, CancellationToken ct = default);
    Task<ServiceResult> SetActiveAsync(int id, bool active, int userId, CancellationToken ct = default);
}