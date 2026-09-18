using FVN_REGISTER.Application.Interfaces.OTReasonCodes;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Application.Maps;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;


namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTReasonCodeManagementService
         : CodeKeyedManagementService<F03OTReasonCode, int, OTReasonCodeDto, OTReasonCodeUpsertDto>,
           IOTReasonCodeManagementService
    {
        public OTReasonCodeManagementService(
            IUnitOfWork uow,
            ILogger<CodeKeyedManagementService<F03OTReasonCode, int, OTReasonCodeDto, OTReasonCodeUpsertDto>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, logger, options)
        {
        }

        protected override string EntityLabel => "Lý do tăng ca (OT Reason)";

        protected override string GetCode(F03OTReasonCode entity) => entity.ReasonCode;
        protected override bool GetIsActive(F03OTReasonCode entity) => entity.IsActive == true;
        protected override void SetIsActive(F03OTReasonCode entity, bool value) => entity.IsActive = value;
        protected override void TouchModified(F03OTReasonCode entity, int currentUserId)
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
        }

        protected override int GetUpsertKey(OTReasonCodeUpsertDto model) => model.Id;
        protected override string GetUpsertCode(OTReasonCodeUpsertDto model) => model.ReasonCode;

        protected override OTReasonCodeDto ToDto(F03OTReasonCode entity) => entity.ToDto();
        protected override F03OTReasonCode ToNewEntity(OTReasonCodeUpsertDto model, int currentUserId)
            => model.ToEntity(currentUserId);
        protected override void ApplyUpsert(F03OTReasonCode entity, OTReasonCodeUpsertDto model, int currentUserId)
            => model.ApplyTo(entity, currentUserId);

        protected override Expression<Func<F03OTReasonCode, bool>> KeyEqualsExpr(int key)
            => x => x.Id == key;
        protected override Expression<Func<F03OTReasonCode, bool>> KeyNotEqualsExpr(int key)
            => x => x.Id != key;
        protected override Expression<Func<F03OTReasonCode, bool>> CodeEqualsExpr(string code)
            => x => x.ReasonCode == code;

        protected override IQueryable<F03OTReasonCode> ApplyActiveFilter(IQueryable<F03OTReasonCode> query, bool isActive)
            => query.Where(x => x.IsActive == isActive);

        protected override IOrderedQueryable<F03OTReasonCode> ApplyDefaultOrder(IQueryable<F03OTReasonCode> query)
            => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.ReasonCode);

        // TODO: khi F03OTRequest có cột ReasonCode, thay bằng kiểm tra thực tế.
        protected override Task<bool> IsInUseAsync(F03OTReasonCode entity, CancellationToken ct)
            => Task.FromResult(false);

        public Task<ServiceResult<List<OTReasonCodeDto>>> GetFilteredAsync(bool? isActive, CancellationToken ct = default)
            => GetFilteredCoreAsync(isActive, extraFilter: null, ct);

    }
}
