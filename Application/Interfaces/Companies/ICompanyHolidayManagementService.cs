


using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Companies
{
    public interface ICompanyHolidayManagementService
    {
        Task<List<CompanyHolidayDto>> GetAllAsync(CancellationToken ct = default);

        Task<CompanyHolidayDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<List<int>> GetWorkYearsAsync(CancellationToken ct = default);

        Task<ServiceResult<CompanyHolidayDto>> CreateAsync(
            CompanyHolidayDto model, int userId, CancellationToken ct = default);

        Task<ServiceResult<CompanyHolidayDto>> UpdateAsync(
            CompanyHolidayDto model, int userId, CancellationToken ct = default);

        Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

        Task<ServiceResult> CreateSundaysAsync(
            int year, int userId, CancellationToken ct = default);
    }
}
