using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Services.HrmSync.ManualSync.BaseManuals;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
using Microsoft.Extensions.Logging;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Importers
{
    public class LeaveTypeStagingImporter
       : HrmStagingImporterBase<HrmLeaveTypeSourceRow, F03StagingLeaveType, F03LeaveType>
    {
        public override string EntityType => "LeaveType"; // khớp LeaveTypeHrmSyncJob.EntityType

        public LeaveTypeStagingImporter(
            IHrmSourceReader<HrmLeaveTypeSourceRow> reader,
            IUnitOfWork uow,
            ILogger<LeaveTypeStagingImporter> logger)
            : base(reader, uow, logger)
        {
        }

        protected override string GetSourceKey(HrmLeaveTypeSourceRow row)
            => row.LeaveTypeCode;

        protected override System.Linq.Expressions.Expression<Func<F03LeaveType, string>> GetEntityKeySelector()
            => x => x.LeaveTypeCode;

        protected override F03StagingLeaveType MapToStaging(HrmLeaveTypeSourceRow row) => new()
        {
            EntityKey = row.LeaveTypeCode,
            LeaveTypeName = row.LeaveTypeName,
            LeaveTypeName2 = row.LeaveTypeName2,
            HRMCode = row.HRMCode,
            TinhPhep = row.TinhPhep
            // KHÔNG map TinhPhep — khớp đúng LeaveTypeHrmSyncJob.MapToNewEntity/ApplyUpdate
            // không đụng field này (business tự cấu hình ở FVN, không phải dữ liệu chủ HRM).
        };

        protected override F03StagingLeaveType BuildDeleteStaging(string entityKey) => new()
        {
            EntityKey = entityKey
            // Delete chỉ cần EntityKey để HrmSyncJob.ApplyDelete nhận diện đúng dòng cần vô hiệu hóa.
        };
    }
}
