using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Positions;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FVN_REGISTER.Infrastructure.Services.Companies
{
    public class PositionManagementService
          : CodeKeyedManagementService<F03Position, string, PositionDto, PositionUpsertDto>,
            IPositionManagementService
    {
        public PositionManagementService(
            IUnitOfWork uow,
            ILogger<CodeKeyedManagementService<F03Position, string, PositionDto, PositionUpsertDto>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, logger, options)
        {
        }

        protected override string EntityLabel => "chức vụ";

        // ===== Mapping — ủy quyền 100% sang PositionMapper, không viết logic map ở đây =====
        protected override PositionDto ToDto(F03Position e) => e.ToDto();

        protected override F03Position ToNewEntity(PositionUpsertDto model, int currentUserId)
            => model.ToEntity(currentUserId);

        protected override void ApplyUpsert(F03Position entity, PositionUpsertDto model, int currentUserId)
            => model.ApplyTo(entity, currentUserId);

        // ===== Field access cho base dùng (không phải mapping, chỉ đọc/ghi field đơn lẻ) =====
        protected override string GetCode(F03Position e) => e.PositionCode;
        protected override bool GetIsActive(F03Position e) => e.IsActive ?? false;
        protected override void SetIsActive(F03Position e, bool value) => e.IsActive = value;

        protected override void TouchModified(F03Position entity, int currentUserId)
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
        }

        protected override string GetUpsertKey(PositionUpsertDto m) => m.PositionCode;
        protected override string GetUpsertCode(PositionUpsertDto m) => m.PositionCode;

        // ===== Expression cho EF Core dịch sang SQL =====
        protected override Expression<Func<F03Position, bool>> KeyEqualsExpr(string key)
            => x => x.PositionCode == key;

        protected override Expression<Func<F03Position, bool>> KeyNotEqualsExpr(string key)
            => x => x.PositionCode != key;

        protected override Expression<Func<F03Position, bool>> CodeEqualsExpr(string code)
            => x => x.PositionCode == code;

        protected override IQueryable<F03Position> ApplyActiveFilter(
            IQueryable<F03Position> query, bool isActive)
            => query.Where(x => x.IsActive == isActive);

        protected override IOrderedQueryable<F03Position> ApplyDefaultOrder(IQueryable<F03Position> query)
            => query.OrderBy(x => x.IsActive == false).ThenBy(x => x.PositionName);

        // ===== Rule đặc thù: chặn xóa/đổi mã nếu còn nhân viên gắn PositionCode =====
        protected override async Task<bool> IsInUseAsync(F03Position entity, CancellationToken ct)
        {
            return await Uow.Repository<F03Employee>().Query()
                .AnyAsync(x => x.PositionCode == entity.PositionCode, ct);
        }

        // ===== GetFilteredAsync — riêng vì Position có thêm điều kiện isApprove =====
        public Task<ServiceResult<List<PositionDto>>> GetFilteredAsync(
            bool? isApprove, bool? isActive, CancellationToken ct = default)
            => GetFilteredCoreAsync(
                isActive,
                extraFilter: isApprove.HasValue
                    ? q => q.Where(x => x.IsApprove == isApprove.Value)
                    : null,
                ct);
    }
}
