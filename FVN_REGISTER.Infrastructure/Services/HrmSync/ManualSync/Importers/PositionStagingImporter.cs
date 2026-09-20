using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Importers
{
    public class PositionStagingImporter
        : HrmStagingImporterBase<HrmPositionSourceRow, F03StagingPosition, F03Position>
    {
        public override string EntityType => "Position";

        public PositionStagingImporter(
            IHrmSourceReader<HrmPositionSourceRow> reader,
            IUnitOfWork uow,
            ILogger<PositionStagingImporter> logger)
            : base(reader, uow, logger)
        {
        }

        protected override string GetSourceKey(HrmPositionSourceRow row)
            => row.PositionCode;

        protected override Expression<Func<F03Position, string>> GetEntityKeySelector()
            => x => x.PositionCode;

        protected override F03StagingPosition MapToStaging(HrmPositionSourceRow row) => new()
        {
            EntityKey = row.PositionCode,
            PositionName = row.PositionName
            // IsApprove/IsAllowApprove KHÔNG map từ HRM — cấu hình nghiệp vụ thuần của FVN.
            // Để mặc định false ở F03StagingPosition, HrmSyncJob sẽ KHÔNG đụng khi Update
            // (xem PositionHrmSyncJob.ApplyUpdate).
        };

        protected override F03StagingPosition BuildDeleteStaging(string entityKey) => new()
        {
            EntityKey = entityKey
        };
    }
}
