using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Depts;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.Companies
{
    public class DepartmentLookupService : IDepartmentLookupService
    {
        // ★ SỬA: đổi từ IUnitOfWork sang FVNWEBAPPContext trực tiếp
        // — theo Quy tắc A: IUnitOfWork chỉ dùng khi có Write, mọi Read-only
        // Service (Report, Lookup, Query) dùng thẳng DbContext.
        private readonly FVNWEBAPPContext _db;

        public DepartmentLookupService(FVNWEBAPPContext db) => _db = db;

        public async Task<List<DeptOption>> GetActiveDepartmentsAsync(CancellationToken ct = default)
      => await _db.Departments
        .AsNoTracking()
        .WhereActiveDept()
        .OrderBy(x => x.DeptName)
        .Select(x => new DeptOption { DeptCode = x.DeptCode, DeptName = x.DeptName })
        .ToListAsync(ct);
    }

}
