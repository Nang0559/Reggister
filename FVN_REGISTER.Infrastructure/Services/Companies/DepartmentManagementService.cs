using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Depts;

using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FVN_REGISTER.Infrastructure.Services.Companies
{
    public class DepartmentManagementService
         : CodeKeyedManagementService<F03Department, int, DepartmentDto, DepartmentUpsertDto>,
           IDepartmentManagementService
    {
        public DepartmentManagementService(
            IUnitOfWork uow,
            ILogger<CodeKeyedManagementService<F03Department, int, DepartmentDto, DepartmentUpsertDto>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, logger, options)
        {
        }

        protected override string EntityLabel => "phòng ban";

        // ===== Mapping — ủy quyền 100% sang DepartmentMapper =====
        protected override DepartmentDto ToDto(F03Department e) => e.ToDto();

        protected override F03Department ToNewEntity(DepartmentUpsertDto model, int currentUserId)
            => model.ToEntity(currentUserId);

        protected override void ApplyUpsert(F03Department entity, DepartmentUpsertDto model, int currentUserId)
            => model.ApplyTo(entity, currentUserId);

        // ===== Field access cho base dùng =====
        protected override string GetCode(F03Department e) => e.DeptCode;
        protected override bool GetIsActive(F03Department e) => e.IsActive == true;
        protected override void SetIsActive(F03Department e, bool value) => e.IsActive = value;

        protected override void TouchModified(F03Department entity, int currentUserId)
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
        }

        protected override int GetUpsertKey(DepartmentUpsertDto m) => m.Id;
        protected override string GetUpsertCode(DepartmentUpsertDto m) => m.DeptCode;

        // ===== Expression cho EF Core dịch sang SQL =====
        protected override Expression<Func<F03Department, bool>> KeyEqualsExpr(int key)
            => x => x.Id == key;

        protected override Expression<Func<F03Department, bool>> KeyNotEqualsExpr(int key)
            => x => x.Id != key;

        protected override Expression<Func<F03Department, bool>> CodeEqualsExpr(string code)
            => x => x.DeptCode == code;

        protected override IQueryable<F03Department> ApplyActiveFilter(
            IQueryable<F03Department> query, bool isActive)
            => query.Where(x => x.IsActive == isActive);

        protected override IOrderedQueryable<F03Department> ApplyDefaultOrder(IQueryable<F03Department> query)
            => query.OrderBy(x => !x.IsActive).ThenBy(x => x.DeptName);

        // ===== Rule đặc thù: chặn xóa/đổi mã nếu còn nhân viên gắn DeptCode =====
        protected override async Task<bool> IsInUseAsync(F03Department entity, CancellationToken ct)
        {
            return await Uow.Repository<F03Employee>().Query()
                .AnyAsync(x => x.DeptCode == entity.DeptCode, ct);
        }

        // ===== GetFilteredAsync — Department chỉ cần isActive, không có điều kiện riêng =====
        public Task<ServiceResult<List<DepartmentDto>>> GetFilteredAsync(
            bool? isActive, CancellationToken ct = default)
            => GetFilteredCoreAsync(isActive, extraFilter: null, ct);
    }
}