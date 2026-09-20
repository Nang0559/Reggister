

using FVN_REGISTER.Application.Interfaces.OTTypes;
using FVN_REGISTER.Contract.Dtos.OTTypeDtos;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Application.Maps;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTTypeManagementService
         : CodeKeyedManagementService<F03OTType, int, OTTypeDto, OTTypeUpsertDto>,
           IOTTypeManagementService
    {
        public OTTypeManagementService(
            IUnitOfWork uow,
            ILogger<CodeKeyedManagementService<F03OTType, int, OTTypeDto, OTTypeUpsertDto>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, logger, options)
        {
        }

        protected override string EntityLabel => "Loại tăng ca (OT Type)";

        // ===== Field access =====
        protected override string GetCode(F03OTType entity) => entity.OTTypeCode;
        protected override bool GetIsActive(F03OTType entity) => entity.IsActive == true;
        protected override void SetIsActive(F03OTType entity, bool value) => entity.IsActive = value;
        protected override void TouchModified(F03OTType entity, int currentUserId)
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
        }

        protected override int GetUpsertKey(OTTypeUpsertDto model) => model.Id;
        protected override string GetUpsertCode(OTTypeUpsertDto model) => model.OTTypeCode;

        // ===== Mapping — ủy quyền 100% sang OTTypeMapper =====
        protected override OTTypeDto ToDto(F03OTType entity) => entity.ToDto();
        protected override F03OTType ToNewEntity(OTTypeUpsertDto model, int currentUserId)
            => model.ToEntity(currentUserId);
        protected override void ApplyUpsert(F03OTType entity, OTTypeUpsertDto model, int currentUserId)
            => model.ApplyTo(entity, currentUserId);

        // ===== Expression cho EF Core =====
        protected override Expression<Func<F03OTType, bool>> KeyEqualsExpr(int key)
            => x => x.Id == key;
        protected override Expression<Func<F03OTType, bool>> KeyNotEqualsExpr(int key)
            => x => x.Id != key;
        protected override Expression<Func<F03OTType, bool>> CodeEqualsExpr(string code)
            => x => x.OTTypeCode == code;

        protected override IQueryable<F03OTType> ApplyActiveFilter(IQueryable<F03OTType> query, bool isActive)
            => query.Where(x => x.IsActive == isActive);

        protected override IOrderedQueryable<F03OTType> ApplyDefaultOrder(IQueryable<F03OTType> query)
            => query.OrderBy(x => x.OTTypeCode);

        protected override Task<bool> IsInUseAsync(F03OTType entity, CancellationToken ct)
            => Uow.Repository<F03OTRequest>().Query()
                .AsNoTracking()
                .AnyAsync(x => x.IsActive == true && x.OTTypeCode == entity.OTTypeCode, ct);

        // ===== Public API theo interface =====
        public Task<ServiceResult<List<OTTypeDto>>> GetFilteredAsync(bool? isActive, CancellationToken ct = default)
            => GetFilteredCoreAsync(isActive, extraFilter: null, ct);

    }
}
