using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.Users
{
    public class EmployeeUserResolver : IEmployeeUserResolver
    {
        private readonly IUnitOfWork _uow;

        public EmployeeUserResolver(IUnitOfWork uow) => _uow = uow;

        public async Task<int?> ResolveUserIdAsync(string employeeCode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(employeeCode)) return null;

            return await _uow.Repository<F03User>().Query()
                .AsNoTracking()
                .Where(u => u.EmployeeCode == employeeCode && u.IsActive == true)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Dictionary<string, int>> ResolveUserIdsAsync(
            IEnumerable<string> employeeCodes, CancellationToken ct = default)
        {
            var codes = employeeCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            if (codes.Count == 0) return new Dictionary<string, int>();

            return await _uow.Repository<F03User>().Query()
                .AsNoTracking()
                .Where(u => codes.Contains(u.EmployeeCode!) && u.IsActive == true)
                .ToDictionaryAsync(u => u.EmployeeCode!, u => u.Id, ct);
        }
    }
}
