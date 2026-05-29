using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;


namespace FVN_REGISTER.Contract.Interfaces.Companies
{
    public interface ICompanyHolidayService
    {
        Task<List<CompanyHoliday>> GetAllAsync();
        Task<CompanyHoliday?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(CompanyHoliday model);
        Task<ServiceResult> UpdateAsync(CompanyHoliday model);
        Task<ServiceResult> DeleteAsync(int id);
        Task<ServiceResult> CreateSundaysAsync(int year);
        Task<List<int>> GetWorkYearsAsync();
    }
}
