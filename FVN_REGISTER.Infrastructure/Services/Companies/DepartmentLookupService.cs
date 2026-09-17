using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Companies
{
    public class DepartmentLookupService : IDepartmentLookupService
    {
        private readonly FVNWEBAPPContext _db;

        public DepartmentLookupService(FVNWEBAPPContext db) => _db = db;

        public async Task<List<DeptOption>> GetActiveDepartmentsAsync(CancellationToken ct = default)
            => await _db.Departments
                .AsNoTracking()
                .WhereActiveDept()
                .OrderBy(x => x.DeptName)
                .Select(x => new DeptOption
                {
                    DeptCode = x.DeptCode,
                    DeptName = x.DeptName
                })
                .ToListAsync(ct);
    }
}
